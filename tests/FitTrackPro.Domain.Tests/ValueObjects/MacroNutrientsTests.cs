namespace FitTrackPro.Domain.Tests.ValueObjects;

using FluentAssertions;
using Xunit;
using FitTrackPro.Domain.ValueObjects;

public class MacroNutrientsTests
{
    [Fact]
    public void Create_WithValidValues_ShouldCreateMacros()
    {
        // Arrange & Act
        var macros = new MacroNutrients(30, 40, 20);

        // Assert
        macros.Protein.Should().Be(30);
        macros.Carbs.Should().Be(40);
        macros.Fat.Should().Be(20);
    }

    [Theory]
    [InlineData(-1, 40, 20)]
    [InlineData(30, -1, 20)]
    [InlineData(30, 40, -1)]
    public void Create_WithNegativeValues_ShouldThrowException(
        decimal protein,
        decimal carbs,
        decimal fat)
    {
        // Act
        Action act = () => new MacroNutrients(protein, carbs, fat);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateCalories_ShouldReturnCorrectValue()
    {
        // Arrange
        var macros = new MacroNutrients(30, 40, 20); // 30*4 + 40*4 + 20*9 = 460

        // Act
        var calories = macros.CalculateCalories();

        // Assert
        calories.Should().Be(460);
    }

    [Fact]
    public void Zero_ShouldReturnAllZeros()
    {
        // Act
        var macros = MacroNutrients.Zero;

        // Assert
        macros.Protein.Should().Be(0);
        macros.Carbs.Should().Be(0);
        macros.Fat.Should().Be(0);
        macros.CalculateCalories().Should().Be(0);
    }
}
