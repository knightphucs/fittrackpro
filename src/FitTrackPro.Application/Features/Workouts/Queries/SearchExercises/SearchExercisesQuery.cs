using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Enums;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Queries.SearchExercises;

public record SearchExercisesQuery : IRequest<Result<PaginatedList<ExerciseDto>>>
{
    public string? SearchTerm { get; init; }
    public ExerciseCategory? Category { get; init; }
    public MuscleGroup? MuscleGroup { get; init; }
    public EquipmentType? Equipment { get; init; }
    public DifficultyLevel? Difficulty { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
