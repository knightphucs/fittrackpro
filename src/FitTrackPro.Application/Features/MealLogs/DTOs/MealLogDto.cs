namespace FitTrackPro.Application.Features.MealLogs.DTOs;

public class MealLogDto
{
    public Guid Id { get; init; }
    public Guid FoodId { get; init; }
    public string FoodName { get; init; } = default!;
    public string? FoodNameVi { get; init; }
    public string MealType { get; init; } = default!;
    public decimal ServingSize { get; init; }
    public string ServingUnit { get; init; } = default!;
    public decimal ServingMultiplier { get; init; }
    public int TotalCalories { get; init; }
    public decimal TotalProtein { get; init; }
    public decimal TotalCarbs { get; init; }
    public decimal TotalFat { get; init; }
    public DateTime LoggedAt { get; init; }
    public string? Notes { get; init; }
}
