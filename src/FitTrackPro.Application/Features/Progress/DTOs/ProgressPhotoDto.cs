namespace FitTrackPro.Application.Features.Progress.DTOs;

public class ProgressPhotoDto
{
    public Guid Id { get; init; }
    public string PhotoUrl { get; init; } = default!;
    public string PhotoType { get; init; } = default!;
    public DateTime TakenAt { get; init; }
    public decimal? Weight { get; init; }
    public string? Notes { get; init; }
}