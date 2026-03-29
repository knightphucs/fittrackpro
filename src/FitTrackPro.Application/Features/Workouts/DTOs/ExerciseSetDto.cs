namespace FitTrackPro.Application.Features.Workouts.DTOs;

public class ExerciseSetDto
{
    public Guid Id { get; init; }
    public int SetNumber { get; init; }
    public decimal? Weight { get; init; }
    public int? Reps { get; init; }
    public int? DurationSeconds { get; init; }
    public decimal? Distance { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsPersonalRecord { get; init; }
}
