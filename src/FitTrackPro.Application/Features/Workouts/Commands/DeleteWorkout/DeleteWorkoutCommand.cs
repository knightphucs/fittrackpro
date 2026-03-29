using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Commands.DeleteWorkout;

public record DeleteWorkoutCommand : IRequest<Result<Unit>>
{
    public Guid UserId { get; init; }
    public Guid WorkoutSessionId { get; init; }
}