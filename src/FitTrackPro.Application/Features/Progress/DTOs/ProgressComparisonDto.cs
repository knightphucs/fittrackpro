namespace FitTrackPro.Application.Features.Progress.DTOs;

public class ProgressComparisonDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int DaysBetween { get; init; }
    
    public decimal StartWeight { get; init; }
    public decimal EndWeight { get; init; }
    public decimal WeightChange { get; init; }
    public decimal WeightChangePercentage { get; init; }
    
    public ProgressPhotoDto? StartPhoto { get; init; }
    public ProgressPhotoDto? EndPhoto { get; init; }
    
    public MeasurementChangesDto MeasurementChanges { get; init; } = default!;
}