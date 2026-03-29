namespace FitTrackPro.Application.Features.MealLogs.Queries.GetDailyMeals;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;

public class GetDailyMealsQueryHandler
    : IRequestHandler<GetDailyMealsQuery, Result<DailyMealsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IMealLogRepository _mealLogRepository;

    public GetDailyMealsQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMealLogRepository mealLogRepository)
    {
        _context = context;
        _cacheService = cacheService;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<DailyMealsDto>> Handle(
        GetDailyMealsQuery request,
        CancellationToken cancellationToken)
    {
        // Try cache
        var dateKey = request.Date.Date.ToString("yyyy-MM-dd");
        var cacheKey = $"meals:daily:{request.UserId}:{dateKey}";
        var cached = await _cacheService.GetAsync<DailyMealsDto>(cacheKey, cancellationToken);

        if (cached != null)
        {
            return Result<DailyMealsDto>.Success(cached);
        }

        // Get date range (start and end of day)
        var startOfDay = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        // Get meal logs for the day
        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(request.UserId, startOfDay, endOfDay, cancellationToken);

        // Map to DTOs
        var meals = mealLogs.Select(log => new MealLogDto
        {
            Id = log.Id,
            FoodId = log.FoodSnapshot.OriginalFoodId,
            FoodName = log.FoodSnapshot.FoodName,
            FoodNameVi = log.FoodSnapshot.FoodNameVi,
            MealType = log.MealType.ToString(),
            ServingSize = log.FoodSnapshot.ServingSize,
            ServingUnit = log.FoodSnapshot.ServingUnit,
            ServingMultiplier = log.FoodSnapshot.ServingMultiplier,
            TotalCalories = log.FoodSnapshot.TotalCalories,
            TotalProtein = (decimal)log.FoodSnapshot.TotalProtein,
            TotalCarbs = (decimal)log.FoodSnapshot.TotalCarbs,
            TotalFat = (decimal)log.FoodSnapshot.TotalFat,
            LoggedAt = log.LoggedAt,
            Notes = log.Notes
        }).ToList();

        // Calculate daily summary
        var totalCalories = meals.Sum(m => m.TotalCalories);
        var totalProtein = meals.Sum(m => m.TotalProtein);
        var totalCarbs = meals.Sum(m => m.TotalCarbs);
        var totalFat = meals.Sum(m => m.TotalFat);

        // Get user's goal
        var goal = await _context.UserGoals
            .Where(g => g.UserId == request.UserId && g.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new DailyMealsDto
        {
            Date = request.Date.Date,
            Meals = meals,
            Summary = new DailySummaryDto
            {
                TotalCalories = totalCalories,
                TotalProtein = (int)totalProtein,
                TotalCarbs = (int)totalCarbs,
                TotalFat = (int)totalFat,
                TargetCalories = goal?.TDEE ?? 2000,
                TargetProtein = goal != null ? (int)goal.TargetMacros.Protein : 150,
                TargetCarbs = goal != null ? (int)goal.TargetMacros.Carbs : 200,
                TargetFat = goal != null ? (int)goal.TargetMacros.Fat : 65,
                CaloriesRemaining = (goal?.TDEE ?? 2000) - totalCalories,
                ProteinPercentage = goal != null ? (int)(totalProtein / goal.TargetMacros.Protein * 100) : 0,
                CarbsPercentage = goal != null ? (int)(totalCarbs / goal.TargetMacros.Carbs * 100) : 0,
                FatPercentage = goal != null ? (int)(totalFat / goal.TargetMacros.Fat * 100) : 0
            }
        };

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<DailyMealsDto>.Success(dto);
    }
}
