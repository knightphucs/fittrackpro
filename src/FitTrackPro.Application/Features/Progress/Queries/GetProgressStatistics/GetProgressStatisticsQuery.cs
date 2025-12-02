namespace FitTrackPro.Application.Features.Progress.Queries.GetProgressStatistics;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Progress.DTOs;

public record GetProgressStatisticsQuery(Guid UserId, int Days = 30) 
    : IRequest<Result<ProgressStatisticsDto>>;