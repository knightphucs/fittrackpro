namespace FitTrackPro.Application.Common.Interfaces;

using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.Queries.SearchFoods;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Application.Features.Workouts.Queries.SearchExercises;

public interface ISearchService
{
    // Foods
    Task<PaginatedList<FoodDto>> AdvancedSearchFoodsAsync(
        AdvancedSearchFoodsQuery query, 
        CancellationToken cancellationToken);
    Task<bool> IndexFoodAsync(
        FoodDto food,
        CancellationToken cancellationToken);
    Task<bool> UpdateFoodInIndexAsync(
        FoodDto food,
        CancellationToken cancellationToken);
    Task<bool> RemoveFoodFromIndexAsync(
        Guid foodId,
        CancellationToken cancellationToken);
    Task<(bool IsSuccess, string? ErrorMessage)> RebuildFoodsIndexAsync(
        List<FoodDto> foods,
        CancellationToken cancellationToken);
    
    Task<List<string>> AutocompleteFoodsAsync(
        string query,
        CancellationToken cancellationToken);

    // Exercises
    Task<PaginatedList<ExerciseDto>> SearchExercisesAsync(
        SearchExercisesQuery query, 
        CancellationToken cancellationToken);
    Task<bool> IndexExerciseAsync(ExerciseDto exercise, CancellationToken cancellationToken);
    Task<(bool IsSuccess, string? ErrorMessage)> RebuildExercisesIndexAsync(
        List<ExerciseDto> exercises, 
        CancellationToken cancellationToken);
}
