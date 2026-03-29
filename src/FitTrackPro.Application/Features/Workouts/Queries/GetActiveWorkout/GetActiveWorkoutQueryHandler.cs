using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetActiveWorkout;

public class GetActiveWorkoutQueryHandler 
    : IRequestHandler<GetActiveWorkoutQuery, Result<WorkoutSessionDto?>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetActiveWorkoutQueryHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<WorkoutSessionDto?>> Handle(
        GetActiveWorkoutQuery request,
        CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetActiveSessionAsync(request.UserId, cancellationToken);

        if (workout == null)
            return Result<WorkoutSessionDto?>.Success(null);

        var dto = new WorkoutSessionDto
        {
            Id = workout.Id,
            Title = workout.Title,
            Notes = workout.Notes,
            StartedAt = workout.StartedAt,
            EndedAt = workout.EndedAt,
            DurationMinutes = workout.DurationMinutes,
            TotalCaloriesBurned = workout.TotalCaloriesBurned,
            Status = workout.Status.ToString(),

            Exercises = workout.Exercises.Select(e => new WorkoutExerciseDto
            {
                Id = e.Id,
                ExerciseId = e.ExerciseId,
                ExerciseName = e.ExerciseName,
                ExerciseNameVi = e.ExerciseNameVi,
                OrderIndex = e.OrderIndex,
                Notes = e.Notes,
                Sets = e.Sets.Select(s => new ExerciseSetDto
                {
                    Id = s.Id,
                    SetNumber = s.SetNumber,
                    Weight = s.Weight,
                    Reps = s.Reps,
                    DurationSeconds = s.DurationSeconds,
                    Distance = s.Distance,
                    IsCompleted = s.IsCompleted,
                    IsPersonalRecord = s.IsPersonalRecord
                }).ToList()
            }).ToList()
        };

        return Result<WorkoutSessionDto?>.Success(dto);
    }
}
