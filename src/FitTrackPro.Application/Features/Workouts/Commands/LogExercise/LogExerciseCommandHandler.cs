using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitTrackPro.Application.Features.Workouts.Commands.LogExercise;

public class LogExerciseCommandHandler 
    : IRequestHandler<LogExerciseCommand, Result<WorkoutExerciseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IPersonalRecordRepository _prRepository;
    private readonly ILogger<LogExerciseCommandHandler> _logger;

    public LogExerciseCommandHandler(IApplicationDbContext context, IWorkoutRepository workoutRepository, IPersonalRecordRepository prRepository, ILogger<LogExerciseCommandHandler> logger)
    {
        _context = context;
        _workoutRepository = workoutRepository;
        _prRepository = prRepository;
        _logger = logger;
    }

    public async Task<Result<WorkoutExerciseDto>> Handle(
        LogExerciseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await _workoutRepository.GetByIdAsync(request.WorkoutSessionId, cancellationToken);

            if (workout == null || workout.UserId != request.UserId)
                return Result<WorkoutExerciseDto>.Failure("Workout session not found");

            if (workout.Status != WorkoutStatus.InProgress)
                return Result<WorkoutExerciseDto>.Failure("Workout session is not active");

            var existingExercise = workout.Exercises
                .FirstOrDefault(e => e.ExerciseId == request.ExerciseId);

            WorkoutExercise currentWorkoutExercise;

            if (existingExercise != null)
            {
                currentWorkoutExercise = existingExercise;
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    currentWorkoutExercise.UpdateNotes(request.Notes);
                }
            }
            else
            {
                var exerciseInfo = await _context.Exercises
                    .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

                if (exerciseInfo == null)
                    return Result<WorkoutExerciseDto>.Failure("Exercise not found");

                // OrderIndex = Max hiện tại + 1
                var nextOrderIndex = workout.Exercises.Count != 0
                    ? workout.Exercises.Max(e => e.OrderIndex) + 1 
                    : 1;

                currentWorkoutExercise = WorkoutExercise.Create(
                    request.ExerciseId,
                    exerciseInfo.Name,
                    exerciseInfo.NameVi,
                    exerciseInfo.ImageUrl,
                    request.Notes);
                
                currentWorkoutExercise.SetOrderIndex(nextOrderIndex);
                workout.AddExercise(currentWorkoutExercise);
            }

            var newlyAddedSets = new List<ExerciseSet>();

            foreach (var setInput in request.Sets)
            {
                var nextSetNumber = currentWorkoutExercise.Sets.Any()
                    ? currentWorkoutExercise.Sets.Max(s => s.SetNumber) + 1
                    : 1;

                var set = ExerciseSet.Create(
                    nextSetNumber,
                    setInput.Weight,
                    setInput.Reps,
                    setInput.DurationSeconds,
                    setInput.Distance);

                currentWorkoutExercise.AddSet(set);
                newlyAddedSets.Add(set);
            }

            await _workoutRepository.UpdateAsync(workout, cancellationToken);

            await CheckAndUpdatePersonalRecords(
                request.UserId, 
                request.ExerciseId, 
                currentWorkoutExercise.ExerciseName,
                newlyAddedSets, 
                workout.Id,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new WorkoutExerciseDto
            {
                Id = currentWorkoutExercise.Id,
                ExerciseId = currentWorkoutExercise.ExerciseId,
                ExerciseName = currentWorkoutExercise.ExerciseName,
                ExerciseNameVi = currentWorkoutExercise.ExerciseNameVi,
                ImageUrl = currentWorkoutExercise.ImageUrl,
                OrderIndex = currentWorkoutExercise.OrderIndex,
                Notes = currentWorkoutExercise.Notes,
                Sets = currentWorkoutExercise.Sets.Select(s => new ExerciseSetDto
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
            };

            return Result<WorkoutExerciseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging exercise: {Message}", ex.Message);
            return Result<WorkoutExerciseDto>.Failure($"System error: {ex.Message}");
        }
    }

    private async Task CheckAndUpdatePersonalRecords(
        Guid userId,
        Guid exerciseId,
        string exerciseName,
        List<ExerciseSet> newSets,
        Guid workoutSessionId,
        CancellationToken cancellationToken)
    {
        var maxWeightSet = newSets
            .Where(s => s.Weight.HasValue)
            .MaxBy(s => s.Weight);

        if (maxWeightSet != null && maxWeightSet.Weight.HasValue)
        {
            var currentBestWeight = await _prRepository.GetBestByTypeAsync(
                userId, exerciseId, PersonalRecordType.MaxWeight, cancellationToken);

            if (currentBestWeight == null || maxWeightSet.Weight.Value > currentBestWeight.Value)
            {
                var newPR = PersonalRecord.Create(
                    userId, exerciseId, exerciseName, PersonalRecordType.MaxWeight,
                    maxWeightSet.Weight.Value, "kg", DateTime.UtcNow, workoutSessionId, 
                    relatedWeight: null);

                await _prRepository.AddAsync(newPR, cancellationToken);
                
                maxWeightSet.MarkAsPersonalRecord(); 
            }
        }

        var maxRepsSet = newSets
            .Where(s => s.Reps.HasValue)
            .MaxBy(s => s.Reps);

        if (maxRepsSet != null && maxRepsSet.Reps.HasValue)
        {
            var currentBestReps = await _prRepository.GetBestByTypeAsync(
                userId, exerciseId, PersonalRecordType.MaxReps, cancellationToken);

            bool isNewRecord = false;

            if (currentBestReps == null)
            {
                isNewRecord = true;
            }
            else
            {
                if ((decimal)maxRepsSet.Reps.Value > currentBestReps.Value)
                {
                    decimal oldRecordWeight = currentBestReps.RelatedWeight ?? 0;
                    decimal newRecordWeight = maxRepsSet.Weight ?? 0;

                    if (newRecordWeight >= oldRecordWeight)
                    {
                        isNewRecord = true;
                    }
                }
            }

            if (isNewRecord)
            {
                var newPR = PersonalRecord.Create(
                    userId, exerciseId, exerciseName, PersonalRecordType.MaxReps,
                    maxRepsSet.Reps.Value, "reps", DateTime.UtcNow, workoutSessionId,
                    relatedWeight: maxRepsSet.Weight);

                await _prRepository.AddAsync(newPR, cancellationToken);
                maxRepsSet.MarkAsPersonalRecord();
            }
        }
    }
}