using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Enums;
using MediatR;

namespace FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;

public record LogMealCommand : IRequest<Result<MealLogDto>>
{
    public Guid UserId { get; init; }
    public Guid FoodId { get; init; }
    public MealType MealType { get; init; }
    public decimal ServingMultiplier { get; init; } = 1.0m;
    public DateTime? LoggedAt { get; init; }
    public string? Notes { get; init; }
}
