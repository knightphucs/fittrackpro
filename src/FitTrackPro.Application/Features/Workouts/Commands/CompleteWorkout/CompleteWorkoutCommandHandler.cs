using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Commands.CompleteWorkout;

public class CompleteWorkoutCommandHandler 
    : IRequestHandler<CompleteWorkoutCommand, Result<WorkoutSessionDto>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public CompleteWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<WorkoutSessionDto>> Handle(
        CompleteWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetByIdAsync(request.WorkoutSessionId, cancellationToken);

        if (workout == null)
            return Result<WorkoutSessionDto>.Failure("Workout session not found");

        if (workout.Status == WorkoutStatus.Completed)
            return Result<WorkoutSessionDto>.Failure("Workout already completed");

        // Calculate calories if not provided
        var caloriesBurned = request.CaloriesBurned ?? CalculateCaloriesBurned(workout);
        var endedAt = request.EndedAt ?? DateTime.UtcNow;

        workout.Complete(endedAt, caloriesBurned);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        var dto = MapToDto(workout);
        return Result<WorkoutSessionDto>.Success(dto);
    }

    private int CalculateCaloriesBurned(WorkoutSession workout)
    {
        // Simple calculation: 5 calories per minute
        var duration = (DateTime.UtcNow - workout.StartedAt).TotalMinutes;

        if (duration < 0) duration = 0;
        return (int)(duration * 5);
    }

    private WorkoutSessionDto MapToDto(WorkoutSession workout)
    {
        return new WorkoutSessionDto
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
    }
}