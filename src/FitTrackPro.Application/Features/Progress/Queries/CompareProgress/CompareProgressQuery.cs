namespace FitTrackPro.Application.Features.Progress.Queries.CompareProgress;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record CompareProgressQuery(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate) 
    : IRequest<Result<ProgressComparisonDto>>;