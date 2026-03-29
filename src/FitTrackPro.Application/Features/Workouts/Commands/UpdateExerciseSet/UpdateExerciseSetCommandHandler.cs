using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Commands.UpdateExerciseSet;

public class UpdateExerciseSetCommandHandler 
    : IRequestHandler<UpdateExerciseSetCommand, Result<ExerciseSetDto>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public UpdateExerciseSetCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<ExerciseSetDto>> Handle(
        UpdateExerciseSetCommand request,
        CancellationToken cancellationToken)
    {
        // Verify set exists and belongs to user
        var workout = await _workoutRepository.GetByIdAsync(request.WorkoutSessionId, cancellationToken);

        if (workout == null || workout.UserId != request.UserId)
            return Result<ExerciseSetDto>.Failure("Workout session not found");

        var targetSet = workout.Exercises
            .SelectMany(e => e.Sets)
            .FirstOrDefault(s => s.Id == request.SetId);

        if (targetSet == null)
            return Result<ExerciseSetDto>.Failure("Exercise set not found");

        // Update fields if provided
        if (request.Weight.HasValue)
            targetSet.UpdateWeight(request.Weight.Value);

        if (request.Reps.HasValue)
            targetSet.UpdateReps(request.Reps.Value);

        if (request.DurationSeconds.HasValue)
            targetSet.UpdateDurationSeconds(request.DurationSeconds.Value);

        if (request.Distance.HasValue)
            targetSet.UpdateDistance(request.Distance.Value);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        // Map to DTO
        var setDto = new ExerciseSetDto
        {
            Id = targetSet.Id,
            SetNumber = targetSet.SetNumber,
            Weight = targetSet.Weight,
            Reps = targetSet.Reps,
            DurationSeconds = targetSet.DurationSeconds,
            Distance = targetSet.Distance,
            IsCompleted = targetSet.IsCompleted,
            IsPersonalRecord = targetSet.IsPersonalRecord
        };

        return Result<ExerciseSetDto>.Success(setDto);
    }
}