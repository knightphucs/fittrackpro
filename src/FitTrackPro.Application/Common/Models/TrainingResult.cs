namespace FitTrackPro.Application.Common.Models;

/// <summary>Kết quả training model để log/hiển thị</summary>
public class TrainingResult
{
    public double MicroAccuracy { get; init; }
    public double MacroAccuracy { get; init; }
    public double LogLoss { get; init; }
    public DateTime TrainedAt { get; init; }
    public int TotalImages { get; init; }
    public int Categories { get; init; }
    public double ElapsedSeconds { get; init; }

    public bool MeetsAccuracyTarget => MicroAccuracy >= 0.85;
}
