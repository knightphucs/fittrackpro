namespace FitTrackPro.Application.Features.Goals.Services;

using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.ValueObjects;

public static class MacroCalculator
{
    public static MacroNutrients Calculate(
        int dailyCalories,
        decimal weightKg,
        WeightGoal goal)
    {
        // Protein: 2g per kg for gain, 1.8g for lose, 1.6g for maintain
        var proteinPerKg = goal switch
        {
            WeightGoal.Gain => 2.0m,
            WeightGoal.Lose => 1.8m,
            _ => 1.6m
        };

        var protein = Math.Round(weightKg * proteinPerKg, 1);
        var proteinCalories = protein * 4;

        // Fat: 25-30% of total calories
        var fatPercentage = 0.27m; // 27%
        var fatCalories = dailyCalories * fatPercentage;
        var fat = Math.Round(fatCalories / 9, 1);

        // Carbs: remaining calories
        var carbCalories = dailyCalories - proteinCalories - fatCalories;
        var carbs = Math.Round(carbCalories / 4, 1);

        return new MacroNutrients(protein, carbs, fat);
    }
}
