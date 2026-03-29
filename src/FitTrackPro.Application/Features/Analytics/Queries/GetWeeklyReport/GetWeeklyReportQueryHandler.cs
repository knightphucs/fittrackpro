namespace FitTrackPro.Application.Features.Analytics.Queries.GetWeeklyReport;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;
using FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Entities;

public class GetWeeklyReportQueryHandler 
    : IRequestHandler<GetWeeklyReportQuery, Result<WeeklyReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ICacheService _cacheService;

    public GetWeeklyReportQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMealLogRepository mealLogRepository)
    {
        _context = context;
        _cacheService = cacheService;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<WeeklyReportDto>> Handle(
        GetWeeklyReportQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = (request.StartDate ?? DateTime.UtcNow.AddDays(-7)).Date;
        var endDate = startDate.AddDays(7);

        // Try cache
        var cacheKey = $"report:weekly:{request.UserId}:{startDate:yyyyMMdd}";
        var cached = await _cacheService.GetAsync<WeeklyReportDto>(cacheKey, cancellationToken);
        
        if (cached != null)
        {
            return Result<WeeklyReportDto>.Success(cached);
        }

        // Get user goal
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == request.UserId && g.IsActive, cancellationToken);

        // Get progress entries
        var progressEntries = await _context.ProgressEntries
            .Where(p => p.UserId == request.UserId && 
                       p.RecordedAt >= startDate && 
                       p.RecordedAt < endDate)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        // Get meal logs
        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(
            request.UserId,
            startDate,
            endDate);

        // Get progress photos
        var photoCount = await _context.ProgressPhotos
            .CountAsync(p => p.UserId == request.UserId &&
                           p.TakenAt >= startDate &&
                           p.TakenAt < endDate, cancellationToken);

        // Calculate nutrition summary
        var nutritionSummary = CalculateNutritionSummary(mealLogs, goal, startDate, endDate);

        // Calculate weight progress
        var (startWeight, endWeight, weightChange) = CalculateWeightProgress(progressEntries, goal);

        // Determine if on track
        var onTrack = IsOnTrack(weightChange, goal);

        // Generate achievements
        var achievements = GenerateAchievements(
            nutritionSummary.DaysLogged, 
            photoCount, 
            progressEntries.Count,
            weightChange,
            goal);

        // Generate recommendations
        var recommendations = GenerateRecommendations(
            nutritionSummary,
            weightChange,
            goal);

        var report = new WeeklyReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = 7,
            StartWeight = startWeight,
            EndWeight = endWeight,
            WeightChange = weightChange,
            OnTrack = onTrack,
            Nutrition = nutritionSummary,
            Activity = new ActivitySummaryDto
            {
                TotalMealsLogged = mealLogs.Count,
                TotalWorkouts = 0, // Placeholder for future workout data
                ProgressPhotos = photoCount,
                WeightCheckins = progressEntries.Count
            },
            Achievements = achievements,
            Recommendations = recommendations
        };

        // Cache for 1 hour
        await _cacheService.SetAsync(cacheKey, report, TimeSpan.FromHours(1), cancellationToken);

        return Result<WeeklyReportDto>.Success(report);
    }

    private static NutritionSummaryDto CalculateNutritionSummary(
        List<MealLog> mealLogs,
        UserGoal? goal,
        DateTime startDate,
        DateTime endDate)
    {
        var dailyStats = mealLogs
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalCalories = g.Sum(m => m.FoodSnapshot.TotalCalories),
                TotalProtein = g.Sum(m => m.FoodSnapshot.TotalProtein),
                TotalCarbs = g.Sum(m => m.FoodSnapshot.TotalCarbs),
                TotalFat = g.Sum(m => m.FoodSnapshot.TotalFat)
            })
            .ToList();

        var daysLogged = dailyStats.Count;
        var totalDays = (endDate - startDate).Days;
        var complianceRate = totalDays > 0 ? (decimal)daysLogged / totalDays * 100 : 0;

        return new NutritionSummaryDto
        {
            AverageDailyCalories = daysLogged > 0 
                ? dailyStats.Sum(d => d.TotalCalories) / daysLogged 
                : 0,
            TargetCalories = goal?.TDEE ?? 2000,
            CalorieDeficit = goal != null 
                ? (goal.TDEE * daysLogged) - dailyStats.Sum(d => d.TotalCalories)
                : 0,
            ProteinAverage = (decimal)(daysLogged > 0 
                ? dailyStats.Average(d => d.TotalProtein) 
                : 0),
            CarbsAverage = (decimal)(daysLogged > 0 
                ? dailyStats.Average(d => d.TotalCarbs) 
                : 0),
            FatAverage = (decimal)(daysLogged > 0 
                ? dailyStats.Average(d => d.TotalFat) 
                : 0),
            DaysLogged = daysLogged,
            ComplianceRate = Math.Round(complianceRate, 1)
        };
    }

    private static (decimal startWeight, decimal endWeight, decimal change) CalculateWeightProgress(
        List<ProgressEntry> entries,
        UserGoal? goal)
    {
        if (entries.Count == 0)
        {
            var currentWeight = goal?.CurrentWeight ?? 0;
            return (currentWeight, currentWeight, 0);
        }

        var startWeight = entries.First().Weight;
        var endWeight = entries.Last().Weight;
        var change = endWeight - startWeight;

        return (startWeight, endWeight, change);
    }

    private static bool IsOnTrack(decimal weightChange, Domain.Entities.UserGoal? goal)
    {
        if (goal == null) return false;

        return goal.WeightGoal switch
        {
            Domain.Enums.WeightGoal.Lose => weightChange < 0,
            Domain.Enums.WeightGoal.Gain => weightChange > 0,
            _ => Math.Abs(weightChange) < 0.5m
        };
    }

    private static List<string> GenerateAchievements(
        int daysLogged,
        int photoCount,
        int weightCheckins,
        decimal weightChange,
        Domain.Entities.UserGoal? goal)
    {
        var achievements = new List<string>();

        if (daysLogged >= 7)
            achievements.Add("🔥 Perfect week! Logged meals every day");
        else if (daysLogged >= 5)
            achievements.Add("💪 Great consistency! Logged 5+ days");

        if (photoCount > 0)
            achievements.Add("📸 Progress photo captured");

        if (weightCheckins >= 3)
            achievements.Add("⚖️ Consistent weight tracking");

        if (goal != null && Math.Abs(weightChange) >= 0.5m)
        {
            if (goal.WeightGoal == Domain.Enums.WeightGoal.Lose && weightChange < 0)
                achievements.Add($"🎯 Lost {Math.Abs(weightChange):F1}kg this week!");
            else if (goal.WeightGoal == Domain.Enums.WeightGoal.Gain && weightChange > 0)
                achievements.Add($"💪 Gained {weightChange:F1}kg this week!");
        }

        if (achievements.Count == 0)
            achievements.Add("🌟 Keep going! Small steps lead to big results");

        return achievements;
    }

    private static List<string> GenerateRecommendations(
        NutritionSummaryDto nutrition,
        decimal weightChange,
        Domain.Entities.UserGoal? goal)
    {
        var recommendations = new List<string>();

        if (nutrition.ComplianceRate < 70)
            recommendations.Add("Try to log meals more consistently for better tracking");

        if (nutrition.AverageDailyCalories > nutrition.TargetCalories + 300)
            recommendations.Add("You're eating above your target. Consider reducing portion sizes");
        else if (nutrition.AverageDailyCalories < nutrition.TargetCalories - 500)
            recommendations.Add("You might be under-eating. Make sure you're getting enough energy");

        if (nutrition.ProteinAverage < goal?.TargetMacros.Protein * 0.8m)
            recommendations.Add("Try to increase protein intake for better muscle maintenance");

        if (goal != null)
        {
            var expectedChange = goal.WeightGoal switch
            {
                Domain.Enums.WeightGoal.Lose => -0.5m,
                Domain.Enums.WeightGoal.Gain => 0.5m,
                _ => 0m
            };

            var difference = Math.Abs(weightChange - expectedChange);
            if (difference > 0.5m)
                recommendations.Add("Your weight change is faster/slower than expected. Consider adjusting calories");
        }

        if (recommendations.Count == 0)
            recommendations.Add("Keep up the great work! You're on the right track 🎯");

        return recommendations;
    }
}