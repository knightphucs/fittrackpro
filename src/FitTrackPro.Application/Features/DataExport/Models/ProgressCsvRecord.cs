namespace FitTrackPro.Application.Features.DataExport.Models;

public class ProgressCsvRecord
{
    public string Date { get; set; } = default!;
    public decimal Weight { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Hips { get; set; }
    public decimal? Arms { get; set; }
    public decimal? Legs { get; set; }
    public string? Notes { get; set; }
}
