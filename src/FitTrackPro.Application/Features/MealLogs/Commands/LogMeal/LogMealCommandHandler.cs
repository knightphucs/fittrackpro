using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;

public class LogMealCommandHandler : IRequestHandler<LogMealCommand, Result<MealLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ICacheService _cacheService;

    public LogMealCommandHandler(IApplicationDbContext context, ICacheService cacheService, IMealLogRepository mealLogRepository)
    {
        _context = context;
        _cacheService = cacheService;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<MealLogDto>> Handle(LogMealCommand request, CancellationToken cancellationToken)
    {
        // Verify food exists
        var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == request.FoodId, cancellationToken);

        if (food == null)
            return Result<MealLogDto>.Failure("Food item not found.");

        // Create and save meal log
        var loggedAt = (request.LoggedAt ?? DateTime.UtcNow).ToUniversalTime();
        var mealLog = MealLog.Create(
            request.UserId,
            food,
            request.MealType,
            food.ServingSize,
            request.ServingMultiplier,
            loggedAt,
            request.Notes);

        await _mealLogRepository.AddAsync(mealLog, cancellationToken);

        // Invalidate daily summary cache
        var dateKey = loggedAt.Date.ToString("yyyy-MM-dd");
        var cacheKey = $"meals:daily{request.UserId}:{dateKey}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        var dto = new MealLogDto
        {
            Id = mealLog.Id,
            FoodId = mealLog.FoodSnapshot.OriginalFoodId,
            FoodName = mealLog.FoodSnapshot.FoodName,
            FoodNameVi = mealLog.FoodSnapshot.FoodNameVi,
            MealType = mealLog.MealType.ToString(),
            ServingSize = mealLog.FoodSnapshot.ServingSize,
            ServingUnit = mealLog.FoodSnapshot.ServingUnit,
            ServingMultiplier = mealLog.FoodSnapshot.ServingMultiplier,
            TotalCalories = mealLog.FoodSnapshot.TotalCalories,
            TotalProtein = (decimal)mealLog.FoodSnapshot.TotalProtein,
            TotalCarbs = (decimal)mealLog.FoodSnapshot.TotalCarbs,
            TotalFat = (decimal)mealLog.FoodSnapshot.TotalFat,
            LoggedAt = mealLog.LoggedAt,
            Notes = mealLog.Notes
        };

        return Result<MealLogDto>.Success(dto);
    }
}
