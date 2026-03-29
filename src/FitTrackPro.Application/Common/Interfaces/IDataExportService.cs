namespace FitTrackPro.Application.Common.Interfaces;

public interface IDataExportService
{
    Task<byte[]> ExportMealLogsToCsvAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProgressToCsvAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportNutritionReportAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportFullDataToCsvAsync(Guid userId, CancellationToken cancellationToken = default);
}