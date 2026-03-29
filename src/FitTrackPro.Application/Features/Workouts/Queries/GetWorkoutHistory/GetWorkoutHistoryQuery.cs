namespace FitTrackPro.Application.Features.Workouts.Queries.GetWorkoutHistory;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;

public record GetWorkoutHistoryQuery(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<WorkoutSessionDto>>>;
