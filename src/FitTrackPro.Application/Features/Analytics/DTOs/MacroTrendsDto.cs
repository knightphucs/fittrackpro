namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class MacroTrendsDto
{
    public decimal AvgProteinPercentage { get; init; }
    public decimal AvgCarbsPercentage { get; init; }
    public decimal AvgFatPercentage { get; init; }
    public string MostConsistentMacro { get; init; } = default!;
}
