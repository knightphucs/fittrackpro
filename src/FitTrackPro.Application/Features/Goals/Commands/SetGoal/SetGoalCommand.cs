using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Goals.DTOs;
using FitTrackPro.Domain.Enums;
using MediatR;

namespace FitTrackPro.Application.Features.Goals.Commands.SetGoal;

public record SetGoalCommand : IRequest<Result<UserGoalDto>>
{
    public Guid UserId { get; init; }
    public decimal CurrentWeight { get; init; }
    public decimal TargetWeight { get; init; }
    public DateTime? TargetDate { get; init; }
    public ActivityLevel ActivityLevel { get; init; }
    public WeightGoal WeightGoal { get; init; }
}
