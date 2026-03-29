namespace FitTrackPro.Application.Features.Workouts.Commands.LogExercise;

using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

public record LogExerciseCommand : IRequest<Result<WorkoutExerciseDto>>
{
    public Guid UserId { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public Guid ExerciseId { get; init; }
    public string? Notes { get; init; }
    public List<SetInput> Sets { get; init; } = new();
}

public class SetInput
{
    public decimal? Weight { get; init; }
    public int? Reps { get; init; }
    public int? DurationSeconds { get; init; }
    public decimal? Distance { get; init; }
}
