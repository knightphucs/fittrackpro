namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class TopFoodDto
{
    public string Name { get; init; } = default!;
    public int TimesLogged { get; init; }
    public int TotalCalories { get; init; }
}
