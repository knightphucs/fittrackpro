namespace FitTrackPro.Application.Features.Goals.Services;

using FitTrackPro.Domain.Enums;

public static class TDEECalculator
{
    public static int Calculate(
        Gender gender,
        int age,
        decimal heightCm,
        decimal weightKg,
        ActivityLevel activityLevel)
    {
        // Calculate BMR using Mifflin-St Jeor Equation
        decimal bmr = gender == Gender.Male
            ? (10 * weightKg) + (6.25m * heightCm) - (5 * age) + 5
            : (10 * weightKg) + (6.25m * heightCm) - (5 * age) - 161;

        // Apply activity multiplier
        var multiplier = activityLevel switch
        {
            ActivityLevel.Sedentary => 1.2m,
            ActivityLevel.Light => 1.375m,
            ActivityLevel.Moderate => 1.55m,
            ActivityLevel.VeryActive => 1.725m,
            ActivityLevel.ExtraActive => 1.9m,
            _ => 1.2m
        };

        return (int)Math.Round(bmr * multiplier);
    }

    public static int AdjustForGoal(int tdee, WeightGoal goal)
    {
        return goal switch
        {
            WeightGoal.Lose => tdee - 500,      // 500 calorie deficit
            WeightGoal.Gain => tdee + 500,      // 500 calorie surplus
            WeightGoal.Maintain => tdee,
            _ => tdee
        };
    }
}
