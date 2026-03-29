using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Commands.CompleteWorkout;

public record CompleteWorkoutCommand : IRequest<Result<WorkoutSessionDto>>
{
    public Guid UserId { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public DateTime? EndedAt { get; init; }
    public int? CaloriesBurned { get; init; }
}
