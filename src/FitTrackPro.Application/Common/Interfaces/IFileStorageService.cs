namespace FitTrackPro.Application.Common.Interfaces;

using Microsoft.AspNetCore.Http;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadWithCompressionAsync(IFormFile file, string fileName, int maxWidth = 1920, int quality = 85, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<List<string>> UploadMultipleAsync(IEnumerable<IFormFile> files, string folderPath, CancellationToken cancellationToken = default);
}