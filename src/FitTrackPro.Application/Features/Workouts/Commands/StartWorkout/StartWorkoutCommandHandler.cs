using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Commands.StartWorkout;

public class StartWorkoutCommandHandler 
    : IRequestHandler<StartWorkoutCommand, Result<WorkoutSessionDto>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public StartWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<WorkoutSessionDto>> Handle(
        StartWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        // Check if user has an active workout
        var activeWorkout = await _workoutRepository.GetActiveSessionAsync(
            request.UserId, 
            cancellationToken);

        if (activeWorkout != null)
        {
            return Result<WorkoutSessionDto>.Failure(
                "You already have an active workout. Please complete or cancel it first.");
        }

        // Create new workout session
        var startedAt = request.StartedAt ?? DateTime.UtcNow;
        var workout = WorkoutSession.Create(
            request.UserId,
            request.Title,
            startedAt,
            request.Notes);

        await _workoutRepository.AddAsync(workout, cancellationToken);

        var dto = new WorkoutSessionDto
        {
            Id = workout.Id,
            Title = workout.Title,
            Notes = workout.Notes,
            StartedAt = workout.StartedAt,
            Status = workout.Status.ToString(),
            Exercises = new List<WorkoutExerciseDto>()
        };

        return Result<WorkoutSessionDto>.Success(dto);
    }
}
