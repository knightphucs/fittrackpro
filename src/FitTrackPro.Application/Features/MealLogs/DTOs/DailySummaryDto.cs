namespace FitTrackPro.Application.Features.MealLogs.DTOs;

public class DailySummaryDto
{
    public int TotalCalories { get; init; }
    public int TotalProtein { get; init; }
    public int TotalCarbs { get; init; }
    public int TotalFat { get; init; }
    public int TargetCalories { get; init; }
    public int TargetProtein { get; init; }
    public int TargetCarbs { get; init; }
    public int TargetFat { get; init; }
    public int CaloriesRemaining { get; init; }
    public int ProteinPercentage { get; init; }
    public int CarbsPercentage { get; init; }
    public int FatPercentage { get; init; }
}
