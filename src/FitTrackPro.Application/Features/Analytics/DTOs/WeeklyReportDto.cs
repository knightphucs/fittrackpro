// DTOs
namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class WeeklyReportDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalDays { get; init; }
    
    // Weight Progress
    public decimal StartWeight { get; init; }
    public decimal EndWeight { get; init; }
    public decimal WeightChange { get; init; }
    public bool OnTrack { get; init; }
    
    public NutritionSummaryDto Nutrition { get; init; } = default!;
    public ActivitySummaryDto Activity { get; init; } = default!;
    public List<string> Achievements { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
}