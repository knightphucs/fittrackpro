namespace FitTrackPro.Application.Tests.Features.Goals.Services;

using FitTrackPro.Application.Features.Goals.Services;
using FitTrackPro.Domain.Enums;
using FluentAssertions;

public class MarcoCalculatorTests
{
    [Fact]
    public void Calculate_ForWeightLoss_ShouldProvideHighProtein()
    {
        // Arrange
        var dailyCalories = 1800;
        var weight = 70m;
        var goal = WeightGoal.Lose;

        // Act
        var macros = MacroCalculator.Calculate(dailyCalories, weight, goal);

        // Assert
        macros.Protein.Should().BeGreaterThan(100); // High protein for weight loss

        // Total calories from macros should roughly equal daily calories
        var totalCalories = macros.CalculateCalories();
        totalCalories.Should().BeCloseTo(dailyCalories, 50);
    }

    [Fact]
    public void Calculate_ForWeightGain_ShouldProvideBalancedMacros()
    {
        // Arrange
        var dailyCalories = 2500;
        var weight = 70m;
        var goal = WeightGoal.Gain;

        // Act
        var macros = MacroCalculator.Calculate(dailyCalories, weight, goal);

        // Assert
        macros.Protein.Should().Be(140); // 2g per kg for muscle gain
        macros.Carbs.Should().BeGreaterThan(macros.Protein); // More carbs for energy

        var totalCalories = macros.CalculateCalories();
        totalCalories.Should().BeCloseTo(dailyCalories, 50);
    }

    [Fact]
    public void Calculate_ShouldNeverHaveNegativeValues()
    {
        // Arrange
        var dailyCalories = 1200; // Very low calories
        var weight = 50m;
        var goal = WeightGoal.Lose;

        // Act
        var macros = MacroCalculator.Calculate(dailyCalories, weight, goal);

        // Assert
        macros.Protein.Should().BeGreaterThan(0);
        macros.Carbs.Should().BeGreaterThan(0);
        macros.Fat.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(2000, 70, WeightGoal.Lose)]
    [InlineData(2500, 80, WeightGoal.Gain)]
    [InlineData(2200, 75, WeightGoal.Maintain)]
    public void Calculate_ShouldMatchDailyCalories(
        int dailyCalories,
        decimal weight,
        WeightGoal goal)
    {
        // Act
        var macros = MacroCalculator.Calculate(dailyCalories, weight, goal);
        var calculatedCalories = macros.CalculateCalories();

        // Assert
        calculatedCalories.Should().BeCloseTo(dailyCalories, 100);
    }
}
