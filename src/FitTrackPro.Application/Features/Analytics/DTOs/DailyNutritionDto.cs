namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class DailyNutritionDto
{
    public DateTime Date { get; init; }
    public int TotalCalories { get; init; }
    public decimal Protein { get; init; }
    public decimal Carbs { get; init; }
    public decimal Fat { get; init; }
    public int MealCount { get; init; }
}
