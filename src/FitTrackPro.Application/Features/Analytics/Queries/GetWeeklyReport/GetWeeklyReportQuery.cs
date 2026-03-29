namespace FitTrackPro.Application.Features.Analytics.Queries.GetWeeklyReport;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Analytics.DTOs;

public record GetWeeklyReportQuery(Guid UserId, DateTime? StartDate = null) 
    : IRequest<Result<WeeklyReportDto>>;