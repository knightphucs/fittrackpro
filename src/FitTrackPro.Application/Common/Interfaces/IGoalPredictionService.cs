using FitTrackPro.Application.Features.Analytics.DTOs;

namespace FitTrackPro.Application.Common.Interfaces;

public interface IGoalPredictionService
{
    Task<GoalPredictionResult?> PredictGoalAchievementAsync(Guid userId);
}