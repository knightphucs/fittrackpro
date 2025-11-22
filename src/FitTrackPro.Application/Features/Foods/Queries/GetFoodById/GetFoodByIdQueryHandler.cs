namespace FitTrackPro.Application.Features.Foods.Queries.GetFoodById;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;

public class GetFoodByIdQueryHandler
    : IRequestHandler<GetFoodByIdQuery, Result<FoodDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFoodByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FoodDetailDto>> Handle(
        GetFoodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var food = await _context.Foods
            .FirstOrDefaultAsync(f => f.Id == request.FoodId, cancellationToken);

        if (food == null)
        {
            return Result<FoodDetailDto>.Failure("Food not found");
        }

        var dto = new FoodDetailDto
        {
            Id = food.Id,
            Name = food.Name,
            NameVi = food.NameVi,
            Category = food.Category,
            ServingSize = food.ServingSize,
            ServingUnit = food.ServingUnit,
            Calories = food.Calories,
            Protein = food.Macros.Protein,
            Carbs = food.Macros.Carbs,
            Fat = food.Macros.Fat,
            Fiber = food.Fiber,
            Sugar = food.Sugar,
            ImageUrl = food.ImageUrl,
            IsUserCreated = food.IsUserCreated
        };

        return Result<FoodDetailDto>.Success(dto);
    }
}
