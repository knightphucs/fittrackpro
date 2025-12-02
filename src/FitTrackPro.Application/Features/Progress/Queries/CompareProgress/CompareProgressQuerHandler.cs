namespace FitTrackPro.Application.Features.Progress.Queries.CompareProgress;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public class CompareProgressQueryHandler 
    : IRequestHandler<CompareProgressQuery, Result<ProgressComparisonDto>>
{
    private readonly IApplicationDbContext _context;

    public CompareProgressQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProgressComparisonDto>> Handle(
        CompareProgressQuery request,
        CancellationToken cancellationToken)
    {
        // Get closest entries to start and end dates
        var startEntry = await _context.ProgressEntries
            .Where(p => p.UserId == request.UserId && p.RecordedAt >= request.StartDate)
            .OrderBy(p => p.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var endEntry = await _context.ProgressEntries
            .Where(p => p.UserId == request.UserId && p.RecordedAt <= request.EndDate)
            .OrderByDescending(p => p.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (startEntry == null || endEntry == null)
        {
            return Result<ProgressComparisonDto>.Failure("Insufficient data for comparison");
        }

        // Get photos for comparison
        var startPhoto = await _context.ProgressPhotos
            .Where(p => p.UserId == request.UserId && 
                       p.TakenAt >= request.StartDate &&
                       p.PhotoType == "Front")
            .OrderBy(p => p.TakenAt)
            .FirstOrDefaultAsync(cancellationToken);

        var endPhoto = await _context.ProgressPhotos
            .Where(p => p.UserId == request.UserId && 
                       p.TakenAt <= request.EndDate &&
                       p.PhotoType == "Front")
            .OrderByDescending(p => p.TakenAt)
            .FirstOrDefaultAsync(cancellationToken);

        var comparison = new ProgressComparisonDto
        {
            StartDate = startEntry.RecordedAt,
            EndDate = endEntry.RecordedAt,
            DaysBetween = (int)(endEntry.RecordedAt - startEntry.RecordedAt).TotalDays,
            
            StartWeight = startEntry.Weight,
            EndWeight = endEntry.Weight,
            WeightChange = endEntry.Weight - startEntry.Weight,
            WeightChangePercentage = Math.Round(
                ((endEntry.Weight - startEntry.Weight) / startEntry.Weight) * 100, 2),

            StartPhoto = startPhoto != null ? new ProgressPhotoDto
            {
                Id = startPhoto.Id,
                PhotoUrl = startPhoto.PhotoUrl,
                PhotoType = startPhoto.PhotoType,
                TakenAt = startPhoto.TakenAt,
                Weight = startPhoto.Weight,
                Notes = startPhoto.Notes
            } : null,

            EndPhoto = endPhoto != null ? new ProgressPhotoDto
            {
                Id = endPhoto.Id,
                PhotoUrl = endPhoto.PhotoUrl,
                PhotoType = endPhoto.PhotoType,
                TakenAt = endPhoto.TakenAt,
                Weight = endPhoto.Weight,
                Notes = endPhoto.Notes
            } : null,

            MeasurementChanges = new MeasurementChangesDto
            {
                ChestChange = GetChange(startEntry.Chest, endEntry.Chest),
                WaistChange = GetChange(startEntry.Waist, endEntry.Waist),
                HipsChange = GetChange(startEntry.Hips, endEntry.Hips),
                ArmsChange = GetChange(startEntry.Arms, endEntry.Arms),
                LegsChange = GetChange(startEntry.Legs, endEntry.Legs)
            }
        };

        return Result<ProgressComparisonDto>.Success(comparison);
    }

    private static decimal? GetChange(decimal? start, decimal? end)
    {
        if (!start.HasValue || !end.HasValue) return null;
        return Math.Round(end.Value - start.Value, 1);
    }
}