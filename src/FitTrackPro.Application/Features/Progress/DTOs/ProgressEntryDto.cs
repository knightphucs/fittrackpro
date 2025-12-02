namespace FitTrackPro.Application.Features.Progress.DTOs;

public class ProgressEntryDto
{
    public Guid Id { get; init; }
    public decimal Weight { get; init; }
    public decimal? BodyFatPercentage { get; init; }
    public decimal? Chest { get; init; }
    public decimal? Waist { get; init; }
    public decimal? Hips { get; init; }
    public decimal? Arms { get; init; }
    public decimal? Legs { get; init; }
    public DateTime RecordedAt { get; init; }
    public string? Notes { get; init; } 
}