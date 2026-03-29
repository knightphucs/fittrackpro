namespace FitTrackPro.Application.Features.Export.Queries.ExportData;

using MediatR;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Enums;

public class ExportDataQueryHandler : IRequestHandler<ExportDataQuery, Result<byte[]>>
{
    private readonly IDataExportService _exportService;

    public ExportDataQueryHandler(IDataExportService exportService)
    {
        _exportService = exportService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportDataQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            byte[] data = request.Type switch
            {
                ExportType.Progress => await _exportService.ExportProgressToCsvAsync(
                    request.UserId,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken),

                ExportType.MealLogs => await _exportService.ExportMealLogsToCsvAsync(
                    request.UserId,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken),

                ExportType.Full => await _exportService.ExportFullDataToCsvAsync(
                    request.UserId,
                    cancellationToken),

                ExportType.NutritionReport => await _exportService.ExportNutritionReportAsync(
                    request.UserId,
                    request.StartDate ?? DateTime.UtcNow.AddDays(-30),
                    request.EndDate ?? DateTime.UtcNow,
                    cancellationToken),

                _ => throw new ArgumentException("Invalid export type")
            };

            return Result<byte[]>.Success(data);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Export failed: {ex.Message}");
        }
    }
}