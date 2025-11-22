namespace FitTrackPro.Application.Features.Foods.Queries.GetCategories;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;

public class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, Result<List<string>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCategoriesQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<List<string>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cacheKey = "foods:categories";
        var cached = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);

        if (cached != null)
        {
            return Result<List<string>>.Success(cached);
        }

        // Get distinct categories
        var categories = await _context.Foods
            .Where(f => f.Category != null)
            .Select(f => f.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        // Cache for 24 hours (categories rarely change)
        await _cacheService.SetAsync(cacheKey, categories, TimeSpan.FromHours(24), cancellationToken);

        return Result<List<string>>.Success(categories);
    }
}
