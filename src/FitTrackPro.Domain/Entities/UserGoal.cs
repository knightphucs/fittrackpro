namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.ValueObjects;

public class UserGoal : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public decimal CurrentWeight { get; private set; } // kg
    public decimal TargetWeight { get; private set; } // kg
    public DateTime? TargetDate { get; private set; }
    public ActivityLevel ActivityLevel { get; private set; }
    public WeightGoal WeightGoal { get; private set; }
    public int TDEE { get; private set; } // Total Daily Energy Expenditure
    public MacroNutrients TargetMacros { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public User User { get; private set; } = default!;

    private UserGoal() { } // EF Core

    public static UserGoal Create(
        Guid userId,
        decimal currentWeight,
        decimal targetWeight,
        DateTime? targetDate,
        ActivityLevel activityLevel,
        WeightGoal weightGoal,
        int tdee,
        MacroNutrients targetMacros)
    {
        if (currentWeight <= 0)
            throw new ArgumentException("Current weight must be positive", nameof(currentWeight));
        if (targetWeight <= 0)
            throw new ArgumentException("Target weight must be positive", nameof(targetWeight));
        if (tdee <= 0)
            throw new ArgumentException("TDEE must be positive", nameof(tdee));

        return new UserGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrentWeight = currentWeight,
            TargetWeight = targetWeight,
            TargetDate = targetDate,
            ActivityLevel = activityLevel,
            WeightGoal = weightGoal,
            TDEE = tdee,
            TargetMacros = targetMacros,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProgress(decimal currentWeight)
    {
        if (currentWeight <= 0)
            throw new ArgumentException("Weight must be positive", nameof(currentWeight));

        CurrentWeight = currentWeight;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetWeightDifference() => TargetWeight - CurrentWeight;

    public bool IsGoalAchieved() =>
        WeightGoal switch
        {
            WeightGoal.Lose => CurrentWeight <= TargetWeight,
            WeightGoal.Gain => CurrentWeight >= TargetWeight,
            _ => Math.Abs(CurrentWeight - TargetWeight) < 0.5m
        };
}
