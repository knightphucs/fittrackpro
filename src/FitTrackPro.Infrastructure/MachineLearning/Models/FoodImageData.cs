namespace FitTrackPro.Infrastructure.MachineLearning.Models;
 
using Microsoft.ML.Data;

public class FoodImageData
{
    [LoadColumn(0)]
    public string ImagePath { get; set; } = default!;
 
    [LoadColumn(1)]
    public string Label { get; set; } = default!; // e.g. "pho_bo", "com_tam", "banh_mi"
}
 
public class FoodImagePrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = default!;
 
    [ColumnName("Score")]
    public float[] Score { get; set; } = default!;
}