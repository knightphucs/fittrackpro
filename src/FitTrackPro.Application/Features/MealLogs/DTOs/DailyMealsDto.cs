namespace FitTrackPro.Application.Features.MealLogs.DTOs;

public class DailyMealsDto
{
    public DateTime Date { get; init; }
    public List<MealLogDto> Meals { get; init; } = new();
    public DailySummaryDto Summary { get; init; } = default!;
}
