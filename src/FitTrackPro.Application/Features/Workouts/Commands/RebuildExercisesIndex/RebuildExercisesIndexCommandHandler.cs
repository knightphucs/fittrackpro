namespace FitTrackPro.Application.Features.Workouts.Commands.RebuildExercisesIndex;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RebuildExercisesIndexCommandHandler 
    : IRequestHandler<RebuildExercisesIndexCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public RebuildExercisesIndexCommandHandler(
        IApplicationDbContext context, 
        ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<Result<Unit>> Handle(
        RebuildExercisesIndexCommand request, 
        CancellationToken cancellationToken)
    {
        var exercises = await _context.Exercises
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = exercises.Select(e => new ExerciseDto
        {
            Id = e.Id,
            Name = e.Name,
            NameVi = e.NameVi,
            Description = e.Description,
            Category = e.Category.ToString(),
            PrimaryMuscle = e.PrimaryMuscle.ToString(),
            SecondaryMuscles = e.SecondaryMuscles.Select(m => m.ToString()).ToList(),
            Equipment = e.Equipment.ToString(),
            Difficulty = e.Difficulty.ToString(),
            VideoUrl = e.VideoUrl,
            ImageUrl = e.ImageUrl,
            Instructions = e.Instructions,
            IsUserCreated = e.IsUserCreated
        }).ToList();

        var (isSuccess, errorMessage) = await _searchService.RebuildExercisesIndexAsync(dtos, cancellationToken);

        if (!isSuccess)
        {
            return Result<Unit>.Failure($"Failed to rebuild exercises index: {errorMessage}");
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
