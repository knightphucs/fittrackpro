namespace FitTrackPro.Domain.Entities;

using MongoDB.Bson.Serialization.Attributes;
using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.ValueObjects;
using MongoDB.Bson;

public class MealLog
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    [BsonRepresentation(BsonType.String)]
    public MealType MealType { get; private set; }

    public DateTime LoggedAt { get; private set; }
    public string? Notes { get; private set; }
    public FoodSnapshot FoodSnapshot { get; private set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public MealLog() { }

    public static MealLog Create(
        Guid userId,
        Food orig,
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
            FoodSnapshot = new FoodSnapshot(orig, servingSize, servingMultiplier),
            MealType = mealType,
            LoggedAt = loggedAt,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class FoodSnapshot
{
    public Guid OriginalFoodId { get; private set; }
    public string FoodName { get; private set; } = null!;
    public string? FoodNameVi { get; private set; }
    public string ServingUnit { get; private set; } = null!;
    public decimal ServingSize { get; private set; }
    public decimal ServingMultiplier { get; private set; }
    public int TotalCalories { get; private set; }
    public double TotalProtein { get; private set; }
    public double TotalCarbs { get; private set; }
    public double TotalFat { get; private set; }

    public FoodSnapshot() { }

    public FoodSnapshot(Food food, decimal servingSize, decimal multiplier)
    {
        OriginalFoodId = food.Id;
        FoodName = food.Name;
        FoodNameVi = food.NameVi;
        ServingUnit = food.ServingUnit;
        ServingSize = servingSize;
        ServingMultiplier = multiplier;

        var ratio = (double)(multiplier * (servingSize / food.ServingSize));
        
        TotalCalories = (int)Math.Round(food.Calories * ratio);
        TotalProtein = Math.Round((double)food.Macros.Protein * ratio, 1);
        TotalCarbs = Math.Round((double)food.Macros.Carbs * ratio, 1);
        TotalFat = Math.Round((double)food.Macros.Fat * ratio, 1);
    }
}
