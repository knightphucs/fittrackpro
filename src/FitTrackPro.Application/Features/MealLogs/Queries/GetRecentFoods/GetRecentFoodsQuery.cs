namespace FitTrackPro.Application.Features.MealLogs.Queries.GetRecentFoods;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;

public record GetRecentFoodsQuery(Guid UserId, int Count = 10) 
    : IRequest<Result<List<FoodDto>>>;

public class GetRecentFoodsQueryHandler 
    : IRequestHandler<GetRecentFoodsQuery, Result<List<FoodDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentFoodsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FoodDto>>> Handle(
        GetRecentFoodsQuery request,
        CancellationToken cancellationToken)
    {
        var recentFoods = await _context.MealLogs
            .Where(m => m.UserId == request.UserId)
            .OrderByDescending(m => m.LoggedAt)
            .Select(m => m.Food)
            .Distinct()
            .Take(request.Count)
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
            .ToListAsync(cancellationToken);

        return Result<List<FoodDto>>.Success(recentFoods);
    }
}