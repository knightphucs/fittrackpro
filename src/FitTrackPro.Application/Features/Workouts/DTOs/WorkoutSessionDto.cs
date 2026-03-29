namespace FitTrackPro.Application.Features.Workouts.DTOs;

public class WorkoutSessionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? Notes { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public int DurationMinutes { get; init; }
    public int TotalCaloriesBurned { get; init; }
    public string Status { get; init; } = default!;
    public List<WorkoutExerciseDto> Exercises { get; init; } = new();
}
