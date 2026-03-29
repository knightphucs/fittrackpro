namespace FitTrackPro.Application.Features.Analytics.Queries.GetDashboard;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Repositories;

public class GetDashboardQueryHandler 
    : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ICacheService _cacheService;

    public GetDashboardQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMealLogRepository mealLogRepository)
    {
        _context = context;
        _cacheService = cacheService;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<DashboardDto>> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"dashboard:{request.UserId}";
        var cached = await _cacheService.GetAsync<DashboardDto>(cacheKey, cancellationToken);

        if (cached != null)
        {
            return Result<DashboardDto>.Success(cached);
        }

        // Get today's summary
        var todaysSummary = await GetTodaysSummaryAsync(request.UserId, cancellationToken);

        // Get weekly trends
        var weeklyTrends = await GetWeeklyTrendsAsync(request.UserId, cancellationToken);

        // Get progress summary
        var progressSummary = await GetProgressSummaryAsync(request.UserId, cancellationToken);

        // Get goal status
        var goalStatus = await GetGoalStatusAsync(request.UserId, cancellationToken);

        // Calculate streaks
        var streaks = await CalculateStreaksAsync(request.UserId, cancellationToken);

        // Get recent activities
        var recentActivities = await GetRecentActivitiesAsync(request.UserId, cancellationToken);

        var dashboard = new DashboardDto
        {
            TodaysSummary = todaysSummary,
            WeeklyTrends = weeklyTrends,
            ProgressSummary = progressSummary,
            GoalStatus = goalStatus,
            Streaks = streaks,
            RecentActivities = recentActivities
        };

        await _cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<DashboardDto>.Success(dashboard);
    }

    private async Task<DailyNutritionSummary> GetTodaysSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todaysMeals = await _mealLogRepository.GetByUserIdAndDateRangeAsync(
            userId,
            today,
            tomorrow);

        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive, cancellationToken);

        var totalCalories = todaysMeals.Sum(m => m.FoodSnapshot.TotalCalories);
        var totalProtein = todaysMeals.Sum(m => m.FoodSnapshot.TotalProtein);
        var totalCarbs = todaysMeals.Sum(m => m.FoodSnapshot.TotalCarbs);
        var totalFat = todaysMeals.Sum(m => m.FoodSnapshot.TotalFat);

        var targetCalories = goal?.TDEE ?? 2000;
        var targetProtein = goal?.TargetMacros.Protein ?? 150;
        var targetCarbs = goal?.TargetMacros.Carbs ?? 200;
        var targetFat = goal?.TargetMacros.Fat ?? 65;

        return new DailyNutritionSummary
        {
            CaloriesConsumed = totalCalories,
            CaloriesTarget = targetCalories,
            CaloriesRemaining = targetCalories - totalCalories,
            ProteinGrams = (int)totalProtein,
            ProteinTarget = (int)targetProtein,
            CarbsGrams = (int)totalCarbs,
            CarbsTarget = (int)targetCarbs,
            FatGrams = (int)totalFat,
            FatTarget = (int)targetFat,
            MealsLogged = todaysMeals.Count,
            LastMealTime = todaysMeals.Any() ? todaysMeals.Max(m => m.LoggedAt) : DateTime.MinValue
        };
    }

    private async Task<WeeklyTrends> GetWeeklyTrendsAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
        var today = DateTime.UtcNow.Date.AddDays(1);

        var weeklyMeals = await _mealLogRepository.GetByUserIdAndDateRangeAsync(
            userId,
            weekAgo,
            today);

        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive, cancellationToken);

        var targetCalories = goal?.TDEE ?? 2000;

        // Group by day
        var caloriesByDay = weeklyMeals
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => new DailyCalorieData
            {
                Date = g.Key,
                Calories = g.Sum(m => m.FoodSnapshot.TotalCalories),
                Target = targetCalories,
                OnTrack = Math.Abs(g.Sum(m => m.FoodSnapshot.TotalCalories) - targetCalories) < 200
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Meal type distribution
        var mealTypeDistribution = weeklyMeals
            .GroupBy(m => m.MealType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Top foods
        var topFoods = weeklyMeals
            .GroupBy(m => m.FoodSnapshot.FoodName)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return new WeeklyTrends
        {
            CaloriesByDay = caloriesByDay,
            AverageCalories = caloriesByDay.Any() ? (decimal)caloriesByDay.Average(d => d.Calories) : 0,
            TotalMeals = weeklyMeals.Count,
            MealTypeDistribution = mealTypeDistribution,
            TopFoods = topFoods
        };
    }

    private async Task<ProgressSummary> GetProgressSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var entries = await _context.ProgressEntries
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        var photoCount = await _context.ProgressPhotos
            .CountAsync(p => p.UserId == userId, cancellationToken);

        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive, cancellationToken);

        if (!entries.Any())
        {
            return new ProgressSummary
            {
                DaysTracking = 0,
                TotalProgressEntries = 0,
                PhotosUploaded = photoCount,
                Trend = "No data"
            };
        }

        var firstEntry = entries.First();
        var lastEntry = entries.Last();
        var weightChange = lastEntry.Weight - firstEntry.Weight;
        var daysTracking = (int)(lastEntry.RecordedAt - firstEntry.RecordedAt).TotalDays;

        var trend = weightChange switch
        {
            > 0.5m => "Increasing",
            < -0.5m => "Decreasing",
            _ => "Stable"
        };

        return new ProgressSummary
        {
            CurrentWeight = lastEntry.Weight,
            StartWeight = firstEntry.Weight,
            TargetWeight = goal?.TargetWeight,
            WeightChange = weightChange,
            Trend = trend,
            DaysTracking = daysTracking,
            TotalProgressEntries = entries.Count,
            PhotosUploaded = photoCount
        };
    }

    private async Task<GoalStatus?> GetGoalStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive, cancellationToken);

        if (goal == null) return null;

        var weightDiff = Math.Abs(goal.TargetWeight - goal.CurrentWeight);
        var totalDiff = Math.Abs(goal.TargetWeight - goal.CurrentWeight);
        var progressPercentage = totalDiff > 0 
            ? (1 - (weightDiff / totalDiff)) * 100 
            : 100;

        var daysRemaining = goal.TargetDate.HasValue
            ? Math.Max(0, (int)(goal.TargetDate.Value - DateTime.UtcNow).TotalDays)
            : 0;

        return new GoalStatus
        {
            GoalType = goal.WeightGoal.ToString(),
            ProgressPercentage = Math.Round(progressPercentage, 1),
            DaysRemaining = daysRemaining,
            OnTrack = goal.IsGoalAchieved() || progressPercentage > 50,
            EstimatedCompletion = goal.TargetDate?.ToString("MMM dd, yyyy") ?? "No target date"
        };
    }

    private async Task<StreakData> CalculateStreaksAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var allMealDates = await _mealLogRepository.GetLoggedDatesAsync(userId, cancellationToken);

        if (!allMealDates.Any())
        {
            return new StreakData
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                TotalDaysLogged = 0,
                ConsistencyPercentage = 0
            };
        }

        // Calculate current streak
        var currentStreak = 0;
        var today = DateTime.UtcNow.Date;
        
        for (int i = 0; i <= 365; i++)
        {
            var checkDate = today.AddDays(-i);
            if (allMealDates.Contains(checkDate))
            {
                currentStreak++;
            }
            else if (i > 0) // Allow today to be missing
            {
                break;
            }
        }

        // Calculate longest streak
        var longestStreak = 1;
        var tempStreak = 1;
        
        for (int i = 1; i < allMealDates.Count; i++)
        {
            if ((allMealDates[i - 1] - allMealDates[i]).Days == 1)
            {
                tempStreak++;
                longestStreak = Math.Max(longestStreak, tempStreak);
            }
            else
            {
                tempStreak = 1;
            }
        }

        // Calculate consistency
        var firstLogDate = allMealDates.Last();
        var totalDaysSinceStart = (int)(today - firstLogDate).TotalDays + 1;
        var consistencyPercentage = totalDaysSinceStart > 0
            ? (decimal)allMealDates.Count / totalDaysSinceStart * 100
            : 0;

        return new StreakData
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            TotalDaysLogged = allMealDates.Count,
            ConsistencyPercentage = Math.Round(consistencyPercentage, 1)
        };
    }

    private async Task<List<RecentActivity>> GetRecentActivitiesAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var activities = new List<RecentActivity>();

        // Recent meals (last 5)
        var recentMeals = await _mealLogRepository.GetRecentAsync(userId, 5, cancellationToken);

        activities.AddRange(recentMeals.Select(m => new RecentActivity
        {
            Type = "meal",
            Description = $"Logged {m.FoodSnapshot.FoodName} ({m.FoodSnapshot.TotalCalories} cal)",
            Timestamp = m.LoggedAt
        }));

        // Recent weight entries (last 3)
        var recentWeights = await _context.ProgressEntries
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.RecordedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        activities.AddRange(recentWeights.Select(w => new RecentActivity
        {
            Type = "weight",
            Description = $"Logged weight: {w.Weight} kg",
            Timestamp = w.RecordedAt
        }));

        // Recent photos (last 2)
        var recentPhotos = await _context.ProgressPhotos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.TakenAt)
            .Take(2)
            .ToListAsync(cancellationToken);

        activities.AddRange(recentPhotos.Select(p => new RecentActivity
        {
            Type = "photo",
            Description = $"Uploaded {p.PhotoType} progress photo",
            Timestamp = p.TakenAt
        }));

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();
    }
}
