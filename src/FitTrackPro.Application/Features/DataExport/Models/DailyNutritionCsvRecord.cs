namespace FitTrackPro.Application.Features.DataExport.Models;

public class DailyNutritionCsvRecord
{
    public string Date { get; set; } = default!;
    public int TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }
    public int MealCount { get; set; }
    public int BreakfastCalories { get; set; }
    public int LunchCalories { get; set; }
    public int DinnerCalories { get; set; }
    public int SnackCalories { get; set; }
}