using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Goals.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Goals.Queries.GetCurrentGoal;

public class GetCurrentGoalQueryHandler : IRequestHandler<GetCurrentGoalQuery, Result<UserGoalDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentGoalQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserGoalDto>> Handle(GetCurrentGoalQuery request, CancellationToken cancellationToken)
    {
        var userGoal = await _context.UserGoals
            .FirstOrDefaultAsync(ug => ug.UserId == request.UserId && ug.IsActive == true, cancellationToken);

        if (userGoal == null)
            return Result<UserGoalDto>.Failure("No active goal found for the user");

        var dto = new UserGoalDto
        {
            GoalId = userGoal.Id,
            CurrentWeight = userGoal.CurrentWeight,
            TargetWeight = userGoal.TargetWeight,
            TargetDate = userGoal.TargetDate,
            ActivityLevel = userGoal.ActivityLevel.ToString(),
            WeightGoal = userGoal.WeightGoal.ToString(),
            DailyCalories = userGoal.TDEE,
            TargetProtein = (int)userGoal.TargetMacros.Protein,
            TargetCarbs = (int)userGoal.TargetMacros.Carbs,
            TargetFat = (int)userGoal.TargetMacros.Fat,
            WeightDifference = Math.Abs(userGoal.TargetWeight - userGoal.CurrentWeight),
            IsAchieved = userGoal.IsGoalAchieved()
        };

        return Result<UserGoalDto>.Success(dto);
    }
}
