namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class CalorieTrendDto
{
    public int AverageDaily { get; init; }
    public int Highest { get; init; }
    public int Lowest { get; init; }
    public decimal Variance { get; init; }
    public string Consistency { get; init; } = default!; // High, Medium, Low
}
