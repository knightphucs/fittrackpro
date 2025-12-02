using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using FitTrackPro.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Progress.Commands.LogWeight;

public class LogWeightCommandHandler : IRequestHandler<LogWeightCommand, Result<ProgressEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public LogWeightCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<ProgressEntryDto>> Handle(LogWeightCommand request, CancellationToken cancellationToken)
    {
        var recordAt = request.RecordedAt ?? DateTime.UtcNow;
        var startOfDay = recordAt.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Check if entry already exists for today
        var existingEntry = await _context.ProgressEntries
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.RecordedAt >= startOfDay &&
                p.RecordedAt < endOfDay,
                cancellationToken);
        
        ProgressEntry entry;

        if (existingEntry != null)
        {
            // Update existing entry
            existingEntry.Update(
                request.Weight,
                recordAt,
                notes: request.Notes ?? existingEntry.Notes
            );

            entry = existingEntry;
        }
        else
        {
            // Create new entry
            entry = ProgressEntry.Create(
                request.UserId,
                request.Weight,
                recordAt,
                notes: request.Notes
            );

            _context.ProgressEntries.Add(entry);
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        // Update user's current goal if exists
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == request.UserId && g.IsActive, cancellationToken);

        if (goal != null)
        {
            goal.UpdateProgress(request.Weight);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Invalidate cache
        var cacheKey = $"progress:history:{request.UserId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        var dto = new ProgressEntryDto
        {
            Id = entry.Id,
            Weight = entry.Weight,
            RecordedAt = entry.RecordedAt,
            Notes = entry.Notes
        };

        return Result<ProgressEntryDto>.Success(dto);
    }
}