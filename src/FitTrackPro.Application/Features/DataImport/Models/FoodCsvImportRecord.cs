namespace FitTrackPro.Application.Features.DataImport.Models;

public class FoodCsvImportRecord
{
    public string Name { get; set; } = default!;
    public string? NameVi { get; set; }
    public string? Category { get; set; }
    public decimal ServingSize { get; set; }
    public string? ServingUnit { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public decimal? Fiber { get; set; }
    public decimal? Sugar { get; set; }
}
