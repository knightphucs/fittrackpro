namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class GoalPredictionResult
{
    public float PredictedWeight { get; set; }
    public int EstimatedDaysToGoal { get; set; }
    public DateTime EstimatedAchievementDate { get; set; }
    public float ConfidenceLevel { get; set; } // 0-100
    public string Recommendation { get; set; } = default!;
    public bool IsRealistic { get; set; }
}