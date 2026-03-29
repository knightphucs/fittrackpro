using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitTrackPro.Application.Features.Workouts.Queries.SearchExercises;

public class SearchExercisesQueryHandler 
    : IRequestHandler<SearchExercisesQuery, Result<PaginatedList<ExerciseDto>>>
{
    private readonly ISearchService _searchService;
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchExercisesQueryHandler> _logger;

    public SearchExercisesQueryHandler(
        ISearchService searchService,
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<SearchExercisesQueryHandler> logger)
    {
        _searchService = searchService;
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ExerciseDto>>> Handle(
        SearchExercisesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"exercises:search:{request.SearchTerm}:{request.Category}:{request.MuscleGroup}:{request.Equipment}:{request.Difficulty}:{request.PageNumber}:{request.PageSize}";
        
        try
        {
            var cached = await _cacheService.GetAsync<PaginatedList<ExerciseDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return Result<PaginatedList<ExerciseDto>>.Success(cached);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error, proceeding to source.");
        }

        PaginatedList<ExerciseDto> result;

        try
        {
            result = await _searchService.SearchExercisesAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch failed. Falling back to SQL Database.");
            
            result = await QueryDatabaseFallbackAsync(request, cancellationToken);
        }

        try
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1), cancellationToken);
        }
        catch { /* Ignore cache write errors */ }

        return Result<PaginatedList<ExerciseDto>>.Success(result);
    }

    private async Task<PaginatedList<ExerciseDto>> QueryDatabaseFallbackAsync(
        SearchExercisesQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Exercises.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(term) || 
                (e.NameVi != null && e.NameVi.ToLower().Contains(term)));
        }

        if (request.Category.HasValue) query = query.Where(e => e.Category == request.Category);
        if (request.MuscleGroup.HasValue) query = query.Where(e => e.PrimaryMuscle == request.MuscleGroup);
        if (request.Equipment.HasValue) query = query.Where(e => e.Equipment == request.Equipment);
        if (request.Difficulty.HasValue) query = query.Where(e => e.Difficulty == request.Difficulty);

        var totalCount = await query.CountAsync(cancellationToken);

        var exercisesEntities = await query
            .OrderBy(e => e.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var exercises = exercisesEntities
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                NameVi = e.NameVi,
                Description = e.Description,
                Category = e.Category.ToString(),
                PrimaryMuscle = e.PrimaryMuscle.ToString(),
                SecondaryMuscles = e.SecondaryMuscles.Select(m => m.ToString()).ToList(),
                Equipment = e.Equipment.ToString(),
                Difficulty = e.Difficulty.ToString(),
                VideoUrl = e.VideoUrl,
                ImageUrl = e.ImageUrl,
                Instructions = e.Instructions,
                IsUserCreated = e.IsUserCreated
            })
            .ToList();

        return new PaginatedList<ExerciseDto>(exercises, totalCount, request.PageNumber, request.PageSize);
    }
}