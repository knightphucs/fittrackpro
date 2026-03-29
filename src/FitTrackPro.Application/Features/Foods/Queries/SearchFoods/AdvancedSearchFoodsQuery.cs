namespace FitTrackPro.Application.Features.Foods.Queries.SearchFoods;

using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;

public class AdvancedSearchFoodsQuery : IRequest<Result<PaginatedList<FoodDto>>>
{
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    
    public int? MinCalories { get; init; }
    public int? MaxCalories { get; init; }
    public decimal? MinProtein { get; init; }
    public decimal? MaxProtein { get; init; }
    public decimal? MinCarbs { get; init; }
    public decimal? MaxCarbs { get; init; }
    public decimal? MinFat { get; init; }
    public decimal? MaxFat { get; init; }
    
    // Sorting
    public string? SortBy { get; init; } // "calories", "protein", "carbs", "fat", "name"
    public bool IsDescending { get; init; }
    
    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    
    // User preferences
    public bool OnlyUserCreated { get; init; }
    public bool IncludeRecent { get; init; } // Include recently logged foods
}