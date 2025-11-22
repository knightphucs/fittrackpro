namespace FitTrackPro.Application.Features.Goals.DTOs;

public class UserGoalDto
{
    public Guid GoalId { get; init; }
    public decimal CurrentWeight { get; init; }
    public decimal TargetWeight { get; init; }
    public DateTime? TargetDate { get; init; }
    public string ActivityLevel { get; init; } = default!;
    public string WeightGoal { get; init; } = default!;
    public int DailyCalories { get; init; }
    public int TargetProtein { get; init; }
    public int TargetCarbs { get; init; }
    public int TargetFat { get; init; }
    public decimal WeightDifference { get; init; }
    public bool IsAchieved { get; init; }
}
