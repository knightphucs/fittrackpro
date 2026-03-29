namespace FitTrackPro.Application.Common.Interfaces;

public interface IPdfExportService
{
    Task<byte[]> GenerateProgressReportAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateWeeklyReportAsync(Guid userId, DateTime? startDate, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateMonthlyReportAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
}