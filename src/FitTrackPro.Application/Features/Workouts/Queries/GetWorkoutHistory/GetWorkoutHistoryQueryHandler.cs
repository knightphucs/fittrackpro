namespace FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutHistory;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Repositories;

public class GetWorkoutHistoryQueryHandler 
    : IRequestHandler<GetWorkoutHistoryQuery, Result<PaginatedList<WorkoutSessionDto>>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutHistoryQueryHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<PaginatedList<WorkoutSessionDto>>> Handle(
        GetWorkoutHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (workouts, totalCount) = await _workoutRepository.GetPaginatedHistoryAsync(
            request.UserId,
            request.StartDate,
            request.EndDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = workouts.Select(w => new WorkoutSessionDto
        {
            Id = w.Id,
            Title = w.Title,
            Notes = w.Notes,
            StartedAt = w.StartedAt,
            EndedAt = w.EndedAt,
            DurationMinutes = w.DurationMinutes,
            TotalCaloriesBurned = w.TotalCaloriesBurned,
            Status = w.Status.ToString(),
            Exercises = w.Exercises.Select(e => new WorkoutExerciseDto
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
        }).ToList();

        var result = new PaginatedList<WorkoutSessionDto>(
            dtos,
            (int)totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<WorkoutSessionDto>>.Success(result);
    }
}
