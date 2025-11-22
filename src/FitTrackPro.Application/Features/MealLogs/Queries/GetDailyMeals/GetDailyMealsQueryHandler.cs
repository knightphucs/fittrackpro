namespace FitTrackPro.Application.Features.MealLogs.Queries.GetDailyMeals;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Enums;

public class GetDailyMealsQueryHandler
    : IRequestHandler<GetDailyMealsQuery, Result<DailyMealsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetDailyMealsQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
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
        var mealLogs = await _context.MealLogs
            .Include(m => m.Food)
            .Where(m => m.UserId == request.UserId &&
                       m.LoggedAt >= startOfDay &&
                       m.LoggedAt <= endOfDay)
            .OrderBy(m => m.LoggedAt)
            .ToListAsync(cancellationToken);

        // Group by meal type
        var groupedMeals = mealLogs
            .GroupBy(m => m.MealType)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Calculate totals
        var totalCalories = 0;
        var totalProtein = 0m;
        var totalCarbs = 0m;
        var totalFat = 0m;

        var meals = new List<MealLogDto>();

        foreach (var mealLog in mealLogs)
        {
            var calories = mealLog.CalculateTotalCalories(mealLog.Food);
            var macros = mealLog.CalculateTotalMacros(mealLog.Food);

            totalCalories += calories;
            totalProtein += macros.Protein;
            totalCarbs += macros.Carbs;
            totalFat += macros.Fat;

            meals.Add(new MealLogDto
            {
                Id = mealLog.Id,
                FoodId = mealLog.FoodId,
                FoodName = mealLog.Food.Name,
                FoodNameVi = mealLog.Food.NameVi,
                MealType = mealLog.MealType.ToString(),
                ServingSize = mealLog.ServingSize,
                ServingUnit = mealLog.Food.ServingUnit,
                ServingMultiplier = mealLog.ServingMultiplier,
                TotalCalories = calories,
                TotalProtein = macros.Protein,
                TotalCarbs = macros.Carbs,
                TotalFat = macros.Fat,
                LoggedAt = mealLog.LoggedAt,
                Notes = mealLog.Notes
            });
        }

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
