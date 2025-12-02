namespace FitTrackPro.Infrastructure.Services.FileStorage;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FitTrackPro.Application.Common.Interfaces;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["FileStorage:Azure:ConnectionString"];
        _containerName = configuration["FileStorage:Azure:ContainerName"] ?? "progress-photos";
        
        _blobServiceClient = new BlobServiceClient(connectionString);
        
        // Ensure container exists
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(
        IFormFile file,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = file.ContentType },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadWithCompressionAsync(
        IFormFile file,
        string fileName,
        int maxWidth = 1920,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        // Validate
        if (!ImageProcessor.IsValidImageType(file.ContentType))
        {
            throw new InvalidOperationException("Invalid image type");
        }

        // Compress
        using var inputStream = file.OpenReadStream();
        var compressedBytes = await ImageProcessor.CompressImageAsync(
            inputStream, maxWidth, quality);

        // Upload to Azure
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        using var compressedStream = new MemoryStream(compressedBytes);
        await blobClient.UploadAsync(
            compressedStream,
            new BlobHttpHeaders { ContentType = "image/jpeg" },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteAsync(
        string fileUrl, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobName = uri.Segments[^1]; // Last segment

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]> DownloadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var uri = new Uri(fileUrl);
        var blobName = uri.Segments[^1];

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        using var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream, cancellationToken);
        return stream.ToArray();
    }

    public async Task<List<string>> UploadMultipleAsync(
        IEnumerable<IFormFile> files,
        string folderPath,
        CancellationToken cancellationToken = default)
    {
        var urls = new List<string>();

        foreach (var file in files)
        {
            var fileName = $"{folderPath}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var url = await UploadWithCompressionAsync(file, fileName, cancellationToken: cancellationToken);
            urls.Add(url);
        }

        return urls;
    }
}