namespace FitTrackPro.Application.Features.Analytics.Queries.GetNutritionTrends;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;

public class GetNutritionTrendsQueryHandler 
    : IRequestHandler<GetNutritionTrendsQuery, Result<NutritionTrendsDto>>
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ICacheService _cacheService;

    public GetNutritionTrendsQueryHandler(
        IMealLogRepository mealLogRepository,
        ICacheService cacheService)
    {
        _mealLogRepository = mealLogRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<NutritionTrendsDto>> Handle(
        GetNutritionTrendsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"trends:nutrition:{request.UserId}:{request.Days}";
        var cached = await _cacheService.GetAsync<NutritionTrendsDto>(cacheKey, cancellationToken);
        
        if (cached != null)
        {
            return Result<NutritionTrendsDto>.Success(cached);
        }

        var startDate = DateTime.UtcNow.AddDays(-request.Days).Date;
        var endDate = DateTime.UtcNow.Date.AddDays(1);

        // Get all meal logs for the period
        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(
            request.UserId,
            startDate,
            endDate);

        if (mealLogs.Count == 0)
        {
            return Result<NutritionTrendsDto>.Failure("No nutrition data found for this period");
        }

        // Calculate daily data
        var dailyData = CalculateDailyData(mealLogs);

        // Calculate macro trends
        var macroTrends = CalculateMacroTrends(dailyData);

        // Calculate calorie trends
        var calorieTrend = CalculateCalorieTrend(dailyData);

        // Get top foods
        var topFoods = GetTopFoods(mealLogs);

        // Calculate meal distribution
        var mealDistribution = CalculateMealDistribution(mealLogs);

        var trends = new NutritionTrendsDto
        {
            Period = $"Last {request.Days} days",
            DailyData = dailyData,
            MacroTrends = macroTrends,
            CalorieTrend = calorieTrend,
            TopFoods = topFoods,
            MealDistribution = mealDistribution
        };

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, trends, TimeSpan.FromMinutes(30), cancellationToken);

        return Result<NutritionTrendsDto>.Success(trends);
    }

    private static List<DailyNutritionDto> CalculateDailyData(List<Domain.Entities.MealLog> mealLogs)
    {
        return mealLogs
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => new DailyNutritionDto
            {
                Date = g.Key,
                TotalCalories = g.Sum(m => m.FoodSnapshot.TotalCalories),
                Protein = (decimal)g.Sum(m => m.FoodSnapshot.TotalProtein),
                Carbs = (decimal)g.Sum(m => m.FoodSnapshot.TotalCarbs),
                Fat = (decimal)g.Sum(m => m.FoodSnapshot.TotalFat),
                MealCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    private static MacroTrendsDto CalculateMacroTrends(List<DailyNutritionDto> dailyData)
    {
        var avgProtein = dailyData.Average(d => d.Protein);
        var avgCarbs = dailyData.Average(d => d.Carbs);
        var avgFat = dailyData.Average(d => d.Fat);

        var totalMacros = avgProtein + avgCarbs + avgFat;

        var proteinPercent = totalMacros > 0 ? (avgProtein / totalMacros) * 100 : 0;
        var carbsPercent = totalMacros > 0 ? (avgCarbs / totalMacros) * 100 : 0;
        var fatPercent = totalMacros > 0 ? (avgFat / totalMacros) * 100 : 0;

        // Determine most consistent macro
        var proteinVariance = CalculateVariance(dailyData.Select(d => (double)d.Protein));
        var carbsVariance = CalculateVariance(dailyData.Select(d => (double)d.Carbs));
        var fatVariance = CalculateVariance(dailyData.Select(d => (double)d.Fat));

        var mostConsistent = proteinVariance < carbsVariance && proteinVariance < fatVariance
            ? "Protein"
            : carbsVariance < fatVariance ? "Carbs" : "Fat";

        return new MacroTrendsDto
        {
            AvgProteinPercentage = Math.Round(proteinPercent, 1),
            AvgCarbsPercentage = Math.Round(carbsPercent, 1),
            AvgFatPercentage = Math.Round(fatPercent, 1),
            MostConsistentMacro = mostConsistent
        };
    }

    private static CalorieTrendDto CalculateCalorieTrend(List<DailyNutritionDto> dailyData)
    {
        var calories = dailyData.Select(d => d.TotalCalories).ToList();
        if (calories.Count == 0) return new CalorieTrendDto();

        var average = (int)calories.Average();
        var highest = calories.Max();
        var lowest = calories.Min();
        var variance = (decimal)CalculateVariance(calories.Select(c => (double)c));

        var consistency = variance < 10000 ? "High" : variance < 30000 ? "Medium" : "Low";

        return new CalorieTrendDto
        {
            AverageDaily = average,
            Highest = highest,
            Lowest = lowest,
            Variance = Math.Round(variance, 0),
            Consistency = consistency
        };
    }

    private static List<TopFoodDto> GetTopFoods(List<Domain.Entities.MealLog> mealLogs)
    {
        return mealLogs
            .GroupBy(m => new { m.FoodSnapshot.FoodName, m.FoodSnapshot.OriginalFoodId })
            .Select(g => new TopFoodDto
            {
                Name = g.Key.FoodName,
                TimesLogged = g.Count(),
                TotalCalories = g.Sum(m => m.FoodSnapshot.TotalCalories)
            })
            .OrderByDescending(f => f.TimesLogged)
            .Take(10)
            .ToList();
    }

    private static MealDistributionDto CalculateMealDistribution(List<Domain.Entities.MealLog> mealLogs)
    {
        var totalCalories = mealLogs.Sum(m => m.FoodSnapshot.TotalCalories);

        var byMealType = mealLogs
            .GroupBy(m => m.MealType)
            .ToDictionary(
                g => g.Key,
                g => (Count: g.Count(), Calories: g.Sum(m => m.FoodSnapshot.TotalCalories)));

        return new MealDistributionDto
        {
            BreakfastMeals = byMealType.GetValueOrDefault(MealType.Breakfast).Count,
            LunchMeals = byMealType.GetValueOrDefault(MealType.Lunch).Count,
            DinnerMeals = byMealType.GetValueOrDefault(MealType.Dinner).Count,
            SnackMeals = byMealType.GetValueOrDefault(MealType.Snack).Count,
            BreakfastCaloriesPercent = totalCalories > 0 
                ? Math.Round((decimal)byMealType.GetValueOrDefault(MealType.Breakfast).Calories / totalCalories * 100, 1) 
                : 0,
            LunchCaloriesPercent = totalCalories > 0 
                ? Math.Round((decimal)byMealType.GetValueOrDefault(MealType.Lunch).Calories / totalCalories * 100, 1) 
                : 0,
            DinnerCaloriesPercent = totalCalories > 0 
                ? Math.Round((decimal)byMealType.GetValueOrDefault(MealType.Dinner).Calories / totalCalories * 100, 1) 
                : 0,
            SnackCaloriesPercent = totalCalories > 0 
                ? Math.Round((decimal)byMealType.GetValueOrDefault(MealType.Snack).Calories / totalCalories * 100, 1) 
                : 0
        };
    }

    private static double CalculateVariance(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        if (valuesList.Count == 0) return 0;

        var avg = valuesList.Average();
        return valuesList.Sum(v => Math.Pow(v - avg, 2)) / valuesList.Count;
    }
}