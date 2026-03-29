namespace FitTrackPro.Application.Features.Workouts.DTOs;

public class WorkoutExerciseDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = default!;
    public string? ExerciseNameVi { get; init; }
    public string? ImageUrl { get; init; }
    public int OrderIndex { get; init; }
    public string? Notes { get; init; }
    public List<ExerciseSetDto> Sets { get; init; } = new();
}
