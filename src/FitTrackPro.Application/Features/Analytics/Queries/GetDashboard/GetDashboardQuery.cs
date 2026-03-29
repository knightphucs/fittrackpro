namespace FitTrackPro.Application.Features.Analytics.Queries.GetDashboard;

using MediatR;
using FitTrackPro.Application.Common.Models;

public record GetDashboardQuery(Guid UserId) : IRequest<Result<DashboardDto>>;

public class DashboardDto
{
    public DailyNutritionSummary TodaysSummary { get; init; } = default!;
    public WeeklyTrends WeeklyTrends { get; init; } = default!;
    public ProgressSummary ProgressSummary { get; init; } = default!;
    public GoalStatus? GoalStatus { get; init; }
    public StreakData Streaks { get; init; } = default!;
    public List<RecentActivity> RecentActivities { get; init; } = new();
}

public class DailyNutritionSummary
{
    public int CaloriesConsumed { get; init; }
    public int CaloriesTarget { get; init; }
    public int CaloriesRemaining { get; init; }
    public int ProteinGrams { get; init; }
    public int ProteinTarget { get; init; }
    public int CarbsGrams { get; init; }
    public int CarbsTarget { get; init; }
    public int FatGrams { get; init; }
    public int FatTarget { get; init; }
    public int MealsLogged { get; init; }
    public DateTime LastMealTime { get; init; }
}

public class WeeklyTrends
{
    public List<DailyCalorieData> CaloriesByDay { get; init; } = new();
    public decimal AverageCalories { get; init; }
    public int TotalMeals { get; init; }
    public Dictionary<string, int> MealTypeDistribution { get; init; } = new();
    public List<string> TopFoods { get; init; } = new();
}

public class DailyCalorieData
{
    public DateTime Date { get; init; }
    public int Calories { get; init; }
    public int Target { get; init; }
    public bool OnTrack { get; init; }
}

public class ProgressSummary
{
    public decimal? CurrentWeight { get; init; }
    public decimal? StartWeight { get; init; }
    public decimal? TargetWeight { get; init; }
    public decimal? WeightChange { get; init; }
    public string Trend { get; init; } = default!;
    public int DaysTracking { get; init; }
    public int TotalProgressEntries { get; init; }
    public int PhotosUploaded { get; init; }
}

public class GoalStatus
{
    public string GoalType { get; init; } = default!;
    public decimal ProgressPercentage { get; init; }
    public int DaysRemaining { get; init; }
    public bool OnTrack { get; init; }
    public string EstimatedCompletion { get; init; } = default!;
}

public class StreakData
{
    public int CurrentStreak { get; init; } // Days logged in a row
    public int LongestStreak { get; init; }
    public int TotalDaysLogged { get; init; }
    public decimal ConsistencyPercentage { get; init; }
}

public class RecentActivity
{
    public string Type { get; init; } = default!; // "meal", "weight", "photo"
    public string Description { get; init; } = default!;
    public DateTime Timestamp { get; init; }
}
