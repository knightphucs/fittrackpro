namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressHistory;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record GetProgressHistoryQuery(
    Guid UserId, 
    DateTime? StartDate = null, 
    DateTime? EndDate = null) 
    : IRequest<Result<List<ProgressEntryDto>>>;