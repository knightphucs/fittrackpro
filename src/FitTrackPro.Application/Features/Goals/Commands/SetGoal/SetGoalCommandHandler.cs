using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Goals.DTOs;
using FitTrackPro.Application.Features.Goals.Services;
using FitTrackPro.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Goals.Commands.SetGoal;

public class SetGoalCommandHandler : IRequestHandler<SetGoalCommand, Result<UserGoalDto>>
{
    private readonly IApplicationDbContext _context;
    public SetGoalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserGoalDto>> Handle(SetGoalCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
            return Result<UserGoalDto>.Failure("User not found.");

        // Validate user has required profile data
        if (!user.DateOfBirth.HasValue || !user.Gender.HasValue || !user.Height.HasValue)
        {
            return Result<UserGoalDto>.Failure(
                "Please complete your profile (date of birth, gender, height) before setting a goal");
        }

        // Deactivate existing goal if any
        if (user.CurrentGoal != null && user.CurrentGoal.IsActive)
        {
            user.CurrentGoal.Deactivate();
        }

        // Calculate TDEE
        var age = user.GetAge()!.Value;
        var tdee = TDEECalculator.Calculate(
            user.Gender!.Value,
            age,
            user.Height.Value,
            request.CurrentWeight,
            request.ActivityLevel);

        // Adjust for goal
        var dailyCalories = TDEECalculator.AdjustForGoal(tdee, request.WeightGoal);

        // Calculate macros
        var macros = MacroCalculator.Calculate(
            dailyCalories,
            request.CurrentWeight,
            request.WeightGoal);

        // Create new goal
        var goal = UserGoal.Create(
            user.Id,
            request.CurrentWeight,
            request.TargetWeight,
            request.TargetDate,
            request.ActivityLevel,
            request.WeightGoal,
            dailyCalories,
            macros);

        _context.UserGoals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new UserGoalDto
        {
            GoalId = goal.Id,
            CurrentWeight = goal.CurrentWeight,
            TargetWeight = goal.TargetWeight,
            TargetDate = goal.TargetDate,
            ActivityLevel = goal.ActivityLevel.ToString(),
            WeightGoal = goal.WeightGoal.ToString(),
            DailyCalories = goal.TDEE,
            TargetProtein = (int)goal.TargetMacros.Protein,
            TargetCarbs = (int)goal.TargetMacros.Carbs,
            TargetFat = (int)goal.TargetMacros.Fat,
            WeightDifference = goal.GetWeightDifference(),
            IsAchieved = goal.IsGoalAchieved()
        };

        return Result<UserGoalDto>.Success(dto);
    }
}