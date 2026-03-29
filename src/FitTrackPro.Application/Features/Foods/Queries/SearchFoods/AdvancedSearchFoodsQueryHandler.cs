using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitTrackPro.Application.Features.Foods.Queries.SearchFoods;

public class AdvancedSearchFoodsQueryHandler : IRequestHandler<AdvancedSearchFoodsQuery, Result<PaginatedList<FoodDto>>>
{
    private readonly ISearchService _searchService;
    private readonly ICacheService _cacheService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdvancedSearchFoodsQueryHandler> _logger;

    public AdvancedSearchFoodsQueryHandler(
        ISearchService searchService,
        ICacheService cacheService,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AdvancedSearchFoodsQueryHandler> logger)
    {
        _searchService = searchService;
        _cacheService = cacheService;
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<FoodDto>>> Handle(AdvancedSearchFoodsQuery request, CancellationToken cancellationToken)
    {
        // Generate Cache Key
        var cacheKey = BuildCacheKey(request);

        // Try Get From Cache
        try
        {
            var cached = await _cacheService.GetAsync<PaginatedList<FoodDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogInformation("Cache HIT for advanced food search: {CacheKey}", cacheKey);
                return Result<PaginatedList<FoodDto>>.Success(cached);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis cache error, proceeding to search source.");
        }

        _logger.LogInformation("Cache MISS. Querying data source...");

        PaginatedList<FoodDto> result;

        // Try Search via Elasticsearch (Primary Source)
        try
        {
            result = await _searchService.AdvancedSearchFoodsAsync(request, cancellationToken);
            
            _logger.LogInformation("Elasticsearch returned {Count} items.", result.Items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch failed. Falling back to Database Query.");
            
            // Fallback: Query Database (EF Core)
            result = await QueryDatabaseFallbackAsync(request, cancellationToken);
        }

        // Set Cache
        try
        {
            var expiration = (string.IsNullOrWhiteSpace(request.SearchTerm) && string.IsNullOrWhiteSpace(request.Category))
                ? TimeSpan.FromHours(2) 
                : TimeSpan.FromHours(1);

            await _cacheService.SetAsync(cacheKey, result, expiration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache.");
        }

        return Result<PaginatedList<FoodDto>>.Success(result);
    }

    private async Task<PaginatedList<FoodDto>> QueryDatabaseFallbackAsync(
        AdvancedSearchFoodsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Foods.AsNoTracking().AsQueryable();

        // Filter: Text Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(searchTerm) ||
                (f.NameVi != null && f.NameVi.ToLower().Contains(searchTerm)));
        }

        // Filter: Category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(f => f.Category == request.Category);
        }

        // Filter: Calories
        if (request.MinCalories.HasValue) query = query.Where(f => f.Calories >= request.MinCalories.Value);
        if (request.MaxCalories.HasValue) query = query.Where(f => f.Calories <= request.MaxCalories.Value);

        // Filter: Macros
        if (request.MinProtein.HasValue) query = query.Where(f => f.Macros.Protein >= request.MinProtein.Value);
        if (request.MaxProtein.HasValue) query = query.Where(f => f.Macros.Protein <= request.MaxProtein.Value);

        // Filter: Carbs
        if (request.MinCarbs.HasValue) query = query.Where(f => f.Macros.Carbs >= request.MinCarbs.Value);
        if (request.MaxCarbs.HasValue) query = query.Where(f => f.Macros.Carbs <= request.MaxCarbs.Value);

        // Filter: Fat
        if (request.MinFat.HasValue) query = query.Where(f => f.Macros.Fat >= request.MinFat.Value);
        if (request.MaxFat.HasValue) query = query.Where(f => f.Macros.Fat <= request.MaxFat.Value);

        // Filter: User Created
        if (request.OnlyUserCreated && _currentUserService.UserId.HasValue)
        {
            query = query.Where(f => f.CreatedByUserId == _currentUserService.UserId);
        }

        // Sorting
        query = ApplySorting(query, request.SortBy, request.IsDescending);

        // Pagination
        var totalCount = await query.CountAsync(cancellationToken);
        var foods = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FoodDto
            {
                Id = f.Id,
                Name = f.Name,
                NameVi = f.NameVi,
                Category = f.Category,
                ServingSize = f.ServingSize,
                ServingUnit = f.ServingUnit,
                Calories = f.Calories,
                Protein = f.Macros.Protein,
                Carbs = f.Macros.Carbs,
                Fat = f.Macros.Fat,
                Fiber = f.Fiber,
                Sugar = f.Sugar,
                ImageUrl = f.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<FoodDto>(foods, totalCount, request.PageNumber, request.PageSize);
    }

    private static IQueryable<Food> ApplySorting(IQueryable<Food> query, string? sortBy, bool isDescending)
    {
        return (sortBy?.ToLower()) switch
        {
            "calories" => isDescending ? query.OrderByDescending(f => f.Calories) : query.OrderBy(f => f.Calories),
            "protein" => isDescending ? query.OrderByDescending(f => f.Macros.Protein) : query.OrderBy(f => f.Macros.Protein),
            "carbs" => isDescending ? query.OrderByDescending(f => f.Macros.Carbs) : query.OrderBy(f => f.Macros.Carbs),
            "fat" => isDescending ? query.OrderByDescending(f => f.Macros.Fat) : query.OrderBy(f => f.Macros.Fat),
            _ => query.OrderBy(f => f.Name) // Default alphabetical
        };
    }

    private string BuildCacheKey(AdvancedSearchFoodsQuery request)
    {
        var userKey = request.OnlyUserCreated ? _currentUserService.UserId?.ToString() ?? "anon" : "global";
        
        return $"foods:adv_search:" +
               $"{request.SearchTerm ?? "*"}:" +
               $"{request.Category ?? "*"}:" +
               $"{request.MinCalories}-{request.MaxCalories}:" +
               $"{request.MinProtein}-{request.MaxProtein}:" +
               $"{request.SortBy}-{request.IsDescending}:" +
               $"{userKey}:" +
               $"p{request.PageNumber}-s{request.PageSize}";
    }
}