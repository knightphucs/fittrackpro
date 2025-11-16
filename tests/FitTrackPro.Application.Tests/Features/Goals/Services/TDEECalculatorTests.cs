namespace FitTrackPro.Application.Tests.Features.Goals.Services;

using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Goals.Services;
using FitTrackPro.Domain.Enums;

public class TDEECalculatorTests
{
    [Theory]
    [InlineData(Gender.Male, 25, 175, 70, ActivityLevel.Sedentary, 2100)]
    [InlineData(Gender.Male, 30, 180, 80, ActivityLevel.Moderate, 2800)]
    [InlineData(Gender.Female, 25, 165, 55, ActivityLevel.Light, 1800)]
    [InlineData(Gender.Female, 35, 160, 60, ActivityLevel.VeryActive, 2200)]
    public void Calculate_WithValidInputs_ShouldReturnExpectedTDEE(
        Gender gender,
        int age,
        decimal height,
        decimal weight,
        ActivityLevel activityLevel,
        int expectedTDEE)
    {
        // Act
        var result = TDEECalculator.Calculate(gender, age, height, weight, activityLevel);

        // Assert
        result.Should().BeCloseTo(expectedTDEE, 100); // Allow 100 calorie variance
    }

    [Fact]
    public void Calculate_MaleVsFemale_FemaleShouldBeLower()
    {
        // Arrange
        var age = 25;
        var height = 170m;
        var weight = 70m;
        var activityLevel = ActivityLevel.Moderate;

        // Act
        var maleTDEE = TDEECalculator.Calculate(Gender.Male, age, height, weight, activityLevel);
        var femaleTDEE = TDEECalculator.Calculate(Gender.Female, age, height, weight, activityLevel);

        // Assert
        maleTDEE.Should().BeGreaterThan(femaleTDEE);
    }

    [Theory]
    [InlineData(2000, WeightGoal.Lose, 1500)]
    [InlineData(2000, WeightGoal.Gain, 2500)]
    [InlineData(2000, WeightGoal.Maintain, 2000)]
    public void AdjustForGoal_ShouldApplyCorrectAdjustment(
        int tdee,
        WeightGoal goal,
        int expectedCalories)
    {
        // Act
        var result = TDEECalculator.AdjustForGoal(tdee, goal);

        // Assert
        result.Should().Be(expectedCalories);
    }
}