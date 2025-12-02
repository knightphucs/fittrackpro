namespace FitTrackPro.Application.Features.Progress.DTOs;

public class ProgressStatisticsDto
{
    public string Period { get; init; } = default!;
    public int TotalEntries { get; init; }
    public decimal StartWeight { get; init; }
    public decimal CurrentWeight { get; init; }
    public decimal? TargetWeight { get; init; }
    public decimal TotalWeightChange { get; init; }
    public decimal AverageWeightChange { get; init; }
    public decimal AverageWeight { get; init; }
    public string Trend { get; init; } = default!; // Increasing, Decreasing, Stable
    public int? WeeksToGoal { get; init; }
    public bool IsOnTrack { get; init; }
    public decimal LowestWeight { get; init; }
    public decimal HighestWeight { get; init; }
    public MeasurementChangesDto? MeasurementChanges { get; init; }
}