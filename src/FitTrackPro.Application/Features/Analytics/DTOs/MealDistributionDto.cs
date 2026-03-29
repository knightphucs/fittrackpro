namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class MealDistributionDto
{
    public int BreakfastMeals { get; init; }
    public int LunchMeals { get; init; }
    public int DinnerMeals { get; init; }
    public int SnackMeals { get; init; }
    public decimal BreakfastCaloriesPercent { get; init; }
    public decimal LunchCaloriesPercent { get; init; }
    public decimal DinnerCaloriesPercent { get; init; }
    public decimal SnackCaloriesPercent { get; init; }
}
