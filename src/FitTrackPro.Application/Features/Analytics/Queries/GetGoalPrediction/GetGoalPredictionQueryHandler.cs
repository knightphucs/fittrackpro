namespace FitTrackPro.Application.Features.Analytics.Queries.GetGoalPrediction;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;
using FitTrackPro.Application.Common.Interfaces;

public class GetGoalPredictionQueryHandler 
    : IRequestHandler<GetGoalPredictionQuery, Result<GoalPredictionResult>>
{
    private readonly IGoalPredictionService _predictionService;

    public GetGoalPredictionQueryHandler(
        IGoalPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    public async Task<Result<GoalPredictionResult>> Handle(
        GetGoalPredictionQuery request,
        CancellationToken cancellationToken)
    {
        var prediction = await _predictionService.PredictGoalAchievementAsync(request.UserId);

        if (prediction == null)
        {
            return Result<GoalPredictionResult>.Failure(
                "Unable to generate prediction. Need at least 3 weight entries and an active goal");
        }

        return Result<GoalPredictionResult>.Success(prediction);
    }
}
