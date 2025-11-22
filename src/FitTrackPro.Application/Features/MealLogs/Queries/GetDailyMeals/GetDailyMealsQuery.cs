namespace FitTrackPro.Application.Features.MealLogs.Queries.GetDailyMeals;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;

public record GetDailyMealsQuery(Guid UserId, DateTime Date)
    : IRequest<Result<DailyMealsDto>>;
