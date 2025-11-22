namespace FitTrackPro.Application.Features.Foods.DTOs;

public class FoodDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? NameVi { get; init; }
    public string? Category { get; init; }
    public decimal ServingSize { get; init; }
    public string ServingUnit { get; init; } = default!;
    public int Calories { get; init; }
    public decimal Protein { get; init; }
    public decimal Carbs { get; init; }
    public decimal Fat { get; init; }
    public decimal? Fiber { get; init; }
    public decimal? Sugar { get; init; }
    public string? ImageUrl { get; init; }
}
