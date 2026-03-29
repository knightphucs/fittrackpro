namespace FitTrackPro.Application.Common.Interfaces;

public interface IDataImportService
{
    Task<ImportResult> ImportFoodsFromCsvAsync(Guid userId, Stream csvStream, CancellationToken cancellationToken);
    Task<byte[]> GetFoodImportTemplateAsync(CancellationToken cancellationToken);
}

public class ImportResult
{
    public bool IsSuccess { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}