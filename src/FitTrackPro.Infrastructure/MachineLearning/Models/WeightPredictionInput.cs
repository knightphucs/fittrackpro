namespace FitTrackPro.Infrastructure.MachineLearning.Models;

using Microsoft.ML.Data;

public class WeightPredictionInput
{
    [LoadColumn(0)]
    public float DayNumber { get; set; }

    [LoadColumn(1)]
    public float CurrentWeight { get; set; }

    [LoadColumn(2)]
    public float AverageDailyCalories { get; set; }

    [LoadColumn(3)]
    public float ActivityLevel { get; set; }

    [LoadColumn(4)]
    public float Age { get; set; }

    [LoadColumn(5)]
    public float Gender { get; set; } // 0 = Female, 1 = Male
}
