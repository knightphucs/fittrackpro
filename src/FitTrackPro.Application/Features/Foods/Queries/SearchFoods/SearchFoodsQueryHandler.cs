using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Foods.Queries.SearchFoods;

public class SearchFoodsQueryHandler : IRequestHandler<SearchFoodsQuery, Result<PaginatedList<FoodDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public SearchFoodsQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<PaginatedList<FoodDto>>> Handle(SearchFoodsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"foods:search:{request.SearchTerm}:{request.Category}:{request.PageNumber}:{request.PageSize}";
        var cached = await _cacheService.GetAsync<PaginatedList<FoodDto>>(cacheKey, cancellationToken);

        if (cached != null)
        {
            return Result<PaginatedList<FoodDto>>.Success(cached);
        }

        // Query foods from the database
        var query = _context.Foods.AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(searchTerm) ||
                (f.NameVi != null && f.NameVi.ToLower().Contains(searchTerm)));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(f => f.Category == request.Category);
        }

        // Order by Name
        query = query.OrderBy(f => f.Name);

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
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

        var result = new PaginatedList<FoodDto>(
            foods,
            totalCount,
            request.PageNumber,
            request.PageSize);

        // Cache the result for 1 hour
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1), cancellationToken);

        return Result<PaginatedList<FoodDto>>.Success(result);
    }
}
