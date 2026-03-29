using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutSummary;

public class GetWorkoutSummaryQueryHandler 
    : IRequestHandler<GetWorkoutSummaryQuery, Result<WorkoutSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutSummaryQueryHandler(IApplicationDbContext context, IWorkoutRepository workoutRepository)
    {
        _context = context;
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<WorkoutSummaryDto>> Handle(
        GetWorkoutSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var workouts = await _workoutRepository.GetCompletedWorkoutsAsync(
            request.UserId,
            request.StartDate,
            request.EndDate,
            cancellationToken
        );

        if (!workouts.Any())
        {
            return Result<WorkoutSummaryDto>.Success(new WorkoutSummaryDto
            {
                TotalWorkouts = 0,
                MostFrequentExercise = "N/A"
            });
        }

        var totalMinutes = workouts.Sum(w => w.DurationMinutes);
        var totalCalories = workouts.Sum(w => w.TotalCaloriesBurned);
        var totalExercises = workouts.Sum(w => w.Exercises.Count);
        
        var allSets = workouts
            .SelectMany(w => w.Exercises)
            .SelectMany(e => e.Sets)
            .ToList();

        var totalSets = allSets.Count;

        var totalVolume = allSets
            .Where(s => s.Weight.HasValue && s.Reps.HasValue)
            .Sum(s => s.Weight!.Value * s.Reps!.Value);

        var exerciseFrequency = workouts
            .SelectMany(w => w.Exercises)
            .GroupBy(e => e.ExerciseName)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        var exerciseIds = workouts
            .SelectMany(w => w.Exercises)
            .Select(e => e.ExerciseId)
            .Distinct()
            .ToList();
        
        var muscleGroup = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.PrimaryMuscle.ToString(), cancellationToken);

        var exercisesByMuscleGroup = workouts
            .SelectMany(w => w.Exercises)
            .GroupBy(e => muscleGroup.ContainsKey(e.ExerciseId) ? muscleGroup[e.ExerciseId] : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var summary = new WorkoutSummaryDto
        {
            TotalWorkouts = workouts.Count,
            TotalMinutes = totalMinutes,
            TotalCaloriesBurned = totalCalories,
            TotalExercises = totalExercises,
            TotalSets = totalSets,
            TotalVolumeKg = totalVolume,
            MostFrequentExercise = exerciseFrequency?.Key ?? "N/A",
            ExercisesByMuscleGroup = exercisesByMuscleGroup
        };

        return Result<WorkoutSummaryDto>.Success(summary);
    }
}
