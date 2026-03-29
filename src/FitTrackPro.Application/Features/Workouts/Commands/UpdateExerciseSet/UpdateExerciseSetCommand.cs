using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Commands.UpdateExerciseSet;

public record UpdateExerciseSetCommand : IRequest<Result<ExerciseSetDto>>
{
    public Guid UserId { get; init; }
    public Guid WorkoutSessionId { get; set; }
    public Guid SetId { get; init; }
    public decimal? Weight { get; init; }
    public int? Reps { get; init; }
    public int? DurationSeconds { get; init; }
    public decimal? Distance { get; init; }
}