namespace FitTrackPro.Application.Features.Export.Queries.ExportData;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Enums;

public record ExportDataQuery(
    Guid UserId,
    ExportType Type,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<byte[]>>;