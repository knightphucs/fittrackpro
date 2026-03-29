namespace FitTrackPro.Application.Features.Analytics.Queries.GetNutritionTrends;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;

public record GetNutritionTrendsQuery(Guid UserId, int Days = 30) 
    : IRequest<Result<NutritionTrendsDto>>;