using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Foods.Commands.RebuildFoodsIndex;

public class RebuildFoodsIndexCommandHandler 
    : IRequestHandler<RebuildFoodsIndexCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public RebuildFoodsIndexCommandHandler(
        IApplicationDbContext context,
        ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<Result<bool>> Handle(
        RebuildFoodsIndexCommand request,
        CancellationToken cancellationToken)
    {
        var foods = await _context.Foods
            .Include(f => f.Macros)
            .AsNoTracking()
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
                Fat = f.Macros.Fat,
                Fiber = f.Fiber,
                Sugar = f.Sugar,
                ImageUrl = f.ImageUrl
            })
            .ToListAsync(cancellationToken);

        if (foods.Count == 0)
            return Result<bool>.Success(true);

        var (isSuccess, errorMessage) = await _searchService.RebuildFoodsIndexAsync(
            foods,
            cancellationToken);

        return isSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(errorMessage ?? "Unknown Elasticsearch error");
    }
}
