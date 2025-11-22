using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;

public class LogMealCommandHandler : IRequestHandler<LogMealCommand, Result<MealLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public LogMealCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<MealLogDto>> Handle(LogMealCommand request, CancellationToken cancellationToken)
    {
        // Verify food exists
        var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == request.FoodId, cancellationToken);

        if (food == null)
        {
            return Result<MealLogDto>.Failure("Food item not found.");
        }

        // Create and save meal log
        var loggedAt = (request.LoggedAt ?? DateTime.UtcNow).ToUniversalTime();
        var mealLog = MealLog.Create(
            request.UserId,
            request.FoodId,
            request.MealType,
            food.ServingSize,
            request.ServingMultiplier,
            loggedAt,
            request.Notes);

        _context.MealLogs.Add(mealLog);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate daily summary cache
        var dateKey = loggedAt.Date.ToString("yyyy-MM-dd");
        var cacheKey = $"meals:daily{request.UserId}:{dateKey}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        // Calculate nutrition
        var totalCalories = mealLog.CalculateTotalCalories(food);
        var totalMacros = mealLog.CalculateTotalMacros(food);

        var dto = new MealLogDto
        {
            Id = mealLog.Id,
            FoodId = mealLog.FoodId,
            FoodName = food.Name,
            FoodNameVi = food.NameVi,
            MealType = mealLog.MealType.ToString(),
            ServingSize = mealLog.ServingSize,
            ServingUnit = food.ServingUnit,
            ServingMultiplier = mealLog.ServingMultiplier,
            TotalCalories = totalCalories,
            TotalProtein = totalMacros.Protein,
            TotalCarbs = totalMacros.Carbs,
            TotalFat = totalMacros.Fat,
            LoggedAt = mealLog.LoggedAt,
            Notes = mealLog.Notes
        };

        return Result<MealLogDto>.Success(dto);
    }
}
