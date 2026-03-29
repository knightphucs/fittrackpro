// DTOs
namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class NutritionSummaryDto
{
    public int AverageDailyCalories { get; init; }
    public int TargetCalories { get; init; }
    public int CalorieDeficit { get; init; }
    public decimal ProteinAverage { get; init; }
    public decimal CarbsAverage { get; init; }
    public decimal FatAverage { get; init; }
    public int DaysLogged { get; init; }
    public decimal ComplianceRate { get; init; } // % days logged
}
