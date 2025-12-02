namespace FitTrackPro.Infrastructure.Services.FileStorage;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FitTrackPro.Application.Common.Interfaces;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _uploadPath = configuration["FileStorage:LocalPath"] ?? "uploads";
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000/uploads";
        
        // Ensure directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadAsync(
        IFormFile file, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, fileName);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"{_baseUrl}/{fileName.Replace("\\", "/")}";
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

        if (!ImageProcessor.IsValidImageSize(file.Length))
        {
            throw new InvalidOperationException("File too large");
        }

        // Compress image
        using var inputStream = file.OpenReadStream();

        var compressedBytes = await ImageProcessor.CompressImageAsync(
            inputStream, 
            maxWidth, 
            quality);

        // Save to disk
        var fullPath = Path.Combine(_uploadPath, fileName);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        await File.WriteAllBytesAsync(fullPath, compressedBytes, cancellationToken);

        return $"{_baseUrl}/{fileName.Replace("\\", "/")}";
    }

    public Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(_uploadPath, relativePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<byte[]> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fileName = fileUrl.Replace(_baseUrl + "/", "");
        var fullPath = Path.Combine(_uploadPath, fileName);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found", fileName);
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
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