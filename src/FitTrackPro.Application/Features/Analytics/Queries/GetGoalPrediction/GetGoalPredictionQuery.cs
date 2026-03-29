namespace FitTrackPro.Application.Features.Analytics.Queries.GetGoalPrediction;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;

public record GetGoalPredictionQuery(Guid UserId) : IRequest<Result<GoalPredictionResult>>;