namespace FitTrackPro.Application.Features.MealLogs.Queries.GetRecentFoods;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Domain.Repositories;

public record GetRecentFoodsQuery(Guid UserId, int Count = 10) 
    : IRequest<Result<List<FoodDto>>>;

public class GetRecentFoodsQueryHandler 
    : IRequestHandler<GetRecentFoodsQuery, Result<List<FoodDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;

    public GetRecentFoodsQueryHandler(IApplicationDbContext context, IMealLogRepository mealLogRepository)
    {
        _context = context;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<Result<List<FoodDto>>> Handle(
        GetRecentFoodsQuery request,
        CancellationToken cancellationToken)
    {
        // Get recent food IDs from meal logs
        var recentFoodIds = await _mealLogRepository.GetRecentFoodIdsAsync(request.UserId, request.Count, cancellationToken);

        if (!recentFoodIds.Any())
        {
            return Result<List<FoodDto>>.Success(new List<FoodDto>());
        }

        // Get food details
        var foods = await _context.Foods
            .Where(f => recentFoodIds.Contains(f.Id))
            .ToListAsync(cancellationToken);
        
        // Map to DTOs and maintain order
        var orderedFoods = recentFoodIds
            .Select(id => foods.First(f => f.Id == id))
            .Select(f => new FoodDto
            {
                Id = f.Id,
                Name = f.Name,
                NameVi = f.NameVi,
                Category = f.Category,
                ServingSize = f.ServingSize,
                ServingUnit = f.ServingUnit,
                Calories = f.Calories,
                Protein = f.Macros.Protein,
                Carbs = f.Macros.Carbs,
                Fat = f.Macros.Fat
            })
            .ToList();

        return Result<List<FoodDto>>.Success(orderedFoods);
    }
}