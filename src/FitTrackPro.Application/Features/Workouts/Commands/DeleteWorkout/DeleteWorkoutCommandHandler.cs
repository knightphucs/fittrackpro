using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitTrackPro.Application.Features.Workouts.Commands.DeleteWorkout;

public class DeleteWorkoutCommandHandler 
    : IRequestHandler<DeleteWorkoutCommand, Result<Unit>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public DeleteWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<Unit>> Handle(
        DeleteWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetByIdAsync(request.WorkoutSessionId, cancellationToken);

        if (workout == null || workout.UserId != request.UserId)
            return Result<Unit>.Failure("Workout session not found");

        await _workoutRepository.DeleteAsync(workout.Id, cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}