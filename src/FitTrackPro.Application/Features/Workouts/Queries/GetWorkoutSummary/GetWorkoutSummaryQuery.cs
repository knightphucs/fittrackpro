using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutSummary;

public record GetWorkoutSummaryQuery(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<WorkoutSummaryDto>>;
