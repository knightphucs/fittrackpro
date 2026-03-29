namespace FitTrackPro.Application.Features.Workouts.DTOs;
public class ExerciseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? NameVi { get; init; }
    public string? Description { get; init; }
    public string Category { get; init; } = default!;
    public string PrimaryMuscle { get; init; } = default!;
    public List<string> SecondaryMuscles { get; init; } = new();
    public string Equipment { get; init; } = default!;
    public string Difficulty { get; init; } = default!;
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Instructions { get; init; }
    public bool IsUserCreated { get; init; }
}