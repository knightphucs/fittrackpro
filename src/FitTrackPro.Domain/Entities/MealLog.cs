namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.ValueObjects;

public class MealLog : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid FoodId { get; private set; }
    public MealType MealType { get; private set; }
    public decimal ServingSize { get; private set; }
    public decimal ServingMultiplier { get; private set; } // 1.5 bowls, etc
    public DateTime LoggedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public User User { get; private set; } = default!;
    public Food Food { get; private set; } = default!;

    private MealLog() { } // EF Core

    public static MealLog Create(
        Guid userId,
        Guid foodId,
        MealType mealType,
        decimal servingSize,
        decimal servingMultiplier,
        DateTime loggedAt,
        string? notes = null)
    {
        if (servingSize <= 0)
            throw new ArgumentException("Serving size must be positive", nameof(servingSize));
        if (servingMultiplier <= 0)
            throw new ArgumentException("Serving multiplier must be positive", nameof(servingMultiplier));

        return new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FoodId = foodId,
            MealType = mealType,
            ServingSize = servingSize,
            ServingMultiplier = servingMultiplier,
            LoggedAt = loggedAt,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public int CalculateTotalCalories(Food food)
    {
        var baseCalories = food.Calories;
        var multiplier = ServingMultiplier * (ServingSize / food.ServingSize);
        return (int)Math.Round(baseCalories * multiplier);
    }

    public MacroNutrients CalculateTotalMacros(Food food)
    {
        var multiplier = ServingMultiplier * (ServingSize / food.ServingSize);
        return new MacroNutrients(
            food.Macros.Protein * multiplier,
            food.Macros.Carbs * multiplier,
            food.Macros.Fat * multiplier
        );
    }
}
