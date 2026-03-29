namespace FitTrackPro.Application.Features.Workouts.Commands.StartWorkout;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;

public record StartWorkoutCommand : IRequest<Result<WorkoutSessionDto>>
{
    public Guid UserId { get; init; }
    public string Title { get; init; } = default!;
    public DateTime? StartedAt { get; init; }
    public string? Notes { get; init; }
}
