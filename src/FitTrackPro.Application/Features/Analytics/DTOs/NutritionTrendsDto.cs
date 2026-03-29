namespace FitTrackPro.Application.Features.Analytics.DTOs;

public class NutritionTrendsDto
{
    public string Period { get; init; } = default!;
    public List<DailyNutritionDto> DailyData { get; init; } = new();
    public MacroTrendsDto MacroTrends { get; init; } = default!;
    public CalorieTrendDto CalorieTrend { get; init; } = default!;
    public List<TopFoodDto> TopFoods { get; init; } = new();
    public MealDistributionDto MealDistribution { get; init; } = default!;
}
