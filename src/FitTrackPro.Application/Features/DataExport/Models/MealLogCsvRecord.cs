namespace FitTrackPro.Application.Features.DataExport.Models;

public class MealLogCsvRecord
{
    public string Date { get; set; } = default!;
    public string Time { get; set; } = default!;
    public string MealType { get; set; } = default!;
    public string FoodName { get; set; } = default!;
    public string? FoodNameVi { get; set; }
    public decimal ServingSize { get; set; }
    public string ServingUnit { get; set; } = default!;
    public decimal ServingMultiplier { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public string? Notes { get; set; }
}
