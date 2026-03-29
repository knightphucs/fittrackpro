using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetActiveWorkout;

public record GetActiveWorkoutQuery(Guid UserId) 
    : IRequest<Result<WorkoutSessionDto?>>;
