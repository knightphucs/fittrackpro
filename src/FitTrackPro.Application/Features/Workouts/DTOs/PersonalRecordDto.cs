namespace FitTrackPro.Application.Features.Workouts.DTOs;

public class PersonalRecordDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = default!;
    public string Type { get; init; } = default!;
    public decimal Value { get; init; }
    public string Unit { get; init; } = default!;
    public DateTime AchievedAt { get; init; }
}
