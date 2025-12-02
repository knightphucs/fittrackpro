namespace FitTrackPro.Application.Features.Progress.Commands.LogMeasurements;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using FitTrackPro.Domain.Entities;

public class LogMeasurementsCommandHandler 
    : IRequestHandler<LogMeasurementsCommand, Result<ProgressEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public LogMeasurementsCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<ProgressEntryDto>> Handle(
        LogMeasurementsCommand request,
        CancellationToken cancellationToken)
    {
        var recordedAt = request.RecordedAt ?? DateTime.UtcNow;

        var startOfDay = recordedAt.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Check if entry already exists for today (same user)
        var existingEntry = await _context.ProgressEntries
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.RecordedAt >= startOfDay &&
                p.RecordedAt < endOfDay,
                cancellationToken);

        ProgressEntry entry;

        if (existingEntry != null)
        {
            decimal currentWeight;
            
            if (request.Weight == existingEntry.Weight)
                currentWeight = existingEntry.Weight;
            else
                currentWeight = request.Weight;

            // UPDATE the same entity
            existingEntry.Update(
                currentWeight, 
                recordedAt,
                request.BodyFatPercentage,
                request.Chest,
                request.Waist,
                request.Hips,
                request.Arms,
                request.Legs,
                request.Notes ?? existingEntry.Notes
            );

            entry = existingEntry;
        }
        else
        {
            // CREATE NEW ENTRY
            entry = ProgressEntry.Create(
                request.UserId,
                request.Weight,
                recordedAt,
                request.BodyFatPercentage,
                request.Chest,
                request.Waist,
                request.Hips,
                request.Arms,
                request.Legs,
                request.Notes
            );

            _context.ProgressEntries.Add(entry);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Update goal
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == request.UserId && g.IsActive, cancellationToken);

        if (goal != null)
        {
            goal.UpdateProgress(request.Weight);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Invalidate cache
        await _cacheService.RemoveAsync($"progress:history:{request.UserId}", cancellationToken);

        var dto = new ProgressEntryDto
        {
            Id = entry.Id,
            Weight = entry.Weight,
            BodyFatPercentage = entry.BodyFatPercentage,
            Chest = entry.Chest,
            Waist = entry.Waist,
            Hips = entry.Hips,
            Arms = entry.Arms,
            Legs = entry.Legs,
            RecordedAt = entry.RecordedAt,
            Notes = entry.Notes
        };

        return Result<ProgressEntryDto>.Success(dto);
    }
}