using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressStatistics;

public class GetProgressStatisticsQueryHandler
    : IRequestHandler<GetProgressStatisticsQuery, Result<ProgressStatisticsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProgressStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProgressStatisticsDto>> Handle(GetProgressStatisticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        var entries = await _context.ProgressEntries
            .Where(p => p.UserId == request.UserId && p.RecordedAt >= startDate)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return Result<ProgressStatisticsDto>.Failure("No progress data found");
        }

        var firstEntry = entries.First();
        var lastEntry = entries.Last();

        // Caculate weight change
        var totalWeightChange = lastEntry.Weight - firstEntry.Weight;
        var averageWeightChange = entries.Count > 1 ? totalWeightChange / (entries.Count - 1) : 0;

        // Caculate averages
        var averageWeight = entries.Average(e => e.Weight);

        // Calculate trend (simple linear regression)
        var trend = CalculateTrend(entries);

        // Get goal
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == request.UserId && g.IsActive, cancellationToken);

        // Calculate weeks to goal
        int? weeksToGoal = null;
        if (goal != null && averageWeightChange != 0)
        {
            var remainingWeight = goal.TargetWeight - lastEntry.Weight;
            var weeksNeeded = Math.Abs(remainingWeight / (averageWeightChange * 7));
            weeksToGoal = (int)Math.Ceiling(weeksNeeded);
        }

        var statistics = new ProgressStatisticsDto
        {
            Period = $"Last {request.Days} days",
            TotalEntries = entries.Count,
            StartWeight = firstEntry.Weight,
            CurrentWeight = lastEntry.Weight,
            TargetWeight = goal?.TargetWeight,
            TotalWeightChange = totalWeightChange,
            AverageWeightChange = Math.Round(averageWeightChange, 2),
            AverageWeight = Math.Round(averageWeight, 2),
            Trend = trend,
            WeeksToGoal = weeksToGoal,
            IsOnTrack = goal != null && IsOnTrack(lastEntry.Weight, goal.TargetWeight, goal.WeightGoal),
            LowestWeight = entries.Min(e => e.Weight),
            HighestWeight = entries.Max(e => e.Weight),
            MeasurementChanges = CalculateMeasurementChanges(firstEntry, lastEntry)
        };

        return Result<ProgressStatisticsDto>.Success(statistics);
    }

    private static string CalculateTrend(List<Domain.Entities.ProgressEntry> entries)
    {
        if (entries.Count < 2) return "Stable";

        var recent = entries.TakeLast(7).ToList();
        var older = entries.Take(Math.Min(7, entries.Count - 7)).ToList();

        if (older.Count == 0) return "Stable";

        var recentAvg = recent.Average(e => e.Weight);
        var olderAvg = older.Average(e => e.Weight);

        var change = recentAvg - olderAvg;

        if (Math.Abs(change) < 0.5m) return "Stable";
        return change > 0 ? "Increasing" : "Decreasing";
    }

    private static bool IsOnTrack(decimal currentWeight, decimal targetWeight, Domain.Enums.WeightGoal goal)
    {
        return goal switch
        {
            Domain.Enums.WeightGoal.Lose => currentWeight < targetWeight + 2,
            Domain.Enums.WeightGoal.Gain => currentWeight > targetWeight - 2,
            _ => Math.Abs(currentWeight - targetWeight) < 2
        };
    }

    private static MeasurementChangesDto? CalculateMeasurementChanges(
        Domain.Entities.ProgressEntry first,
        Domain.Entities.ProgressEntry last)
    {
        if (!first.Chest.HasValue && !first.Waist.HasValue) return null;

        return new MeasurementChangesDto
        {
            ChestChange = last.Chest.HasValue && first.Chest.HasValue 
                ? Math.Round(last.Chest.Value - first.Chest.Value, 1) : null,
            WaistChange = last.Waist.HasValue && first.Waist.HasValue 
                ? Math.Round(last.Waist.Value - first.Waist.Value, 1) : null,
            HipsChange = last.Hips.HasValue && first.Hips.HasValue 
                ? Math.Round(last.Hips.Value - first.Hips.Value, 1) : null,
            ArmsChange = last.Arms.HasValue && first.Arms.HasValue 
                ? Math.Round(last.Arms.Value - first.Arms.Value, 1) : null,
            LegsChange = last.Legs.HasValue && first.Legs.HasValue 
                ? Math.Round(last.Legs.Value - first.Legs.Value, 1) : null
        };
    }
}