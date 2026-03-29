namespace FitTrackPro.Application.Features.Foods.DTOs;
 
public class FoodRecognitionDto
{
    /// <summary>Label predict được (e.g. "pho_bo")</summary>
    public string PredictedLabel { get; init; } = default!;
 
    /// <summary>Tên hiển thị tiếng Việt (e.g. "Phở Bò")</summary>
    public string DisplayName { get; init; } = default!;
 
    /// <summary>Độ tin cậy 0–100%</summary>
    public float Confidence { get; init; }
 
    /// <summary>Top 3 gợi ý</summary>
    public List<FoodCandidateDto> TopCandidates { get; init; } = new();
 
    /// <summary>Thông tin dinh dưỡng nếu match được trong DB</summary>
    public FoodNutritionDto? MatchedFood { get; init; }
 
    /// <summary>Model có confident không (>= 70%)</summary>
    public bool IsConfident => Confidence >= 70f;
 
    /// <summary>Gợi ý hành động cho client</summary>
    public string ActionHint => IsConfident
        ? "Nhận diện thành công! Bạn có muốn log món này không?"
        : "Kết quả chưa chắc chắn. Vui lòng chọn lại hoặc nhập tay.";
}
 
public class FoodCandidateDto
{
    public string Label { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public float Confidence { get; init; }
    public Guid? FoodId { get; init; }
    public int? Calories { get; init; }
}
 
public class FoodNutritionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string NameVi { get; init; } = default!;
    public int CaloriesPerServing { get; init; }
    public decimal ServingSize { get; init; }
    public string ServingUnit { get; init; } = default!;
    public decimal Protein { get; init; }
    public decimal Carbs { get; init; }
    public decimal Fat { get; init; }
    public string Category { get; init; } = default!;
}