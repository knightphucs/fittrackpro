using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressHistory;

public class GetProgressHistoryQueryHandler : IRequestHandler<GetProgressHistoryQuery, Result<List<ProgressEntryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetProgressHistoryQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<List<ProgressEntryDto>>> Handle(GetProgressHistoryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"progress:history:{request.UserId}:{request.StartDate}:{request.EndDate}";
        var cached = await _cacheService.GetAsync<List<ProgressEntryDto>>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return Result<List<ProgressEntryDto>>.Success(cached);
        }

        var query = _context.ProgressEntries
            .Where(p => p.UserId == request.UserId);

        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.RecordedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.RecordedAt <= request.EndDate.Value);
        }

        var entries = await query
            .OrderByDescending(p => p.RecordedAt)
            .Select(p => new ProgressEntryDto
            {
                Id = p.Id,
                Weight = p.Weight,
                BodyFatPercentage = p.BodyFatPercentage,
                Chest = p.Chest,
                Waist = p.Waist,
                Hips = p.Hips,
                Arms = p.Arms,
                Legs = p.Legs,
                RecordedAt = p.RecordedAt,
                Notes = p.Notes,
            })
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(cacheKey, entries, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<List<ProgressEntryDto>>.Success(entries);
    }
}