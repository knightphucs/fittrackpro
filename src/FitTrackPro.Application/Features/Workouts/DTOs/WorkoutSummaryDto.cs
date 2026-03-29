namespace FitTrackPro.Application.Features.Workouts.DTOs;

public class WorkoutSummaryDto
{
    public int TotalWorkouts { get; init; }
    public int TotalMinutes { get; init; }
    public int TotalCaloriesBurned { get; init; }
    public int TotalExercises { get; init; }
    public int TotalSets { get; init; }
    public decimal TotalVolumeKg { get; init; }
    public string MostFrequentExercise { get; init; } = default!;
    public Dictionary<string, int> ExercisesByMuscleGroup { get; init; } = new();
}
