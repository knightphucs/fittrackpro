using Microsoft.ML.Data;

namespace FitTrackPro.Infrastructure.MachineLearning.Models;

public class WeightPredictionOutput
{
    [ColumnName("Score")]
    public float PredictedWeight { get; set; }
}
