namespace FitTrackPro.Application.Common.Models;

public class FoodRecognitionResult
{
    public string PredictedLabel { get; init; } = default!;

    /// <summary>Tên hiển thị tiếng Việt, map từ label</summary>
    public string DisplayName { get; init; } = default!;

    /// <summary>Độ tin cậy 0–100%</summary>
    public float Confidence { get; init; }

    /// <summary>Top 3 dự đoán (label + confidence)</summary>
    public List<FoodPredictionCandidate> TopCandidates { get; init; } = new();

    /// <summary>Đã match được Food trong DB chưa</summary>
    public bool IsMatchedInDatabase { get; init; }

    /// <summary>Food ID tìm được trong DB (nullable nếu không match)</summary>
    public Guid? MatchedFoodId { get; init; }

    /// <summary>Calories ước tính (từ DB)</summary>
    public int? EstimatedCalories { get; init; }
}

public class FoodPredictionCandidate
{
    public string Label { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public float Confidence { get; init; }
}
