using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Queries.SearchFoods;

public class SearchFoodsQuery : IRequest<Result<PaginatedList<FoodDto>>>
{
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
