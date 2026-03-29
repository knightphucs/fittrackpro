namespace FitTrackPro.Infrastructure.Services.FileStorage;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly HttpClient _httpClient;
    private readonly CloudinarySettings _settings;

    public CloudinaryFileStorageService(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
        _httpClient = new HttpClient();
    }

    public async Task<string> UploadAsync(IFormFile file, string fileName, CancellationToken cancellationToken = default)
    {
        var (folder, publicId) = GetFolderAndPublicId(fileName);
        await using var stream = file.OpenReadStream();
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            PublicId = publicId,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadWithCompressionAsync(
        IFormFile file, 
        string fileName, 
        int maxWidth = 1920, 
        int quality = 85, 
        CancellationToken cancellationToken = default)
    {
        using var inputStream = file.OpenReadStream();
        var compressedBytes = await ImageProcessor.CompressImageAsync(inputStream, maxWidth, quality);

        using var compressedStream = new MemoryStream(compressedBytes);
        var (folder, publicId) = GetFolderAndPublicId(fileName);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, compressedStream),
            Folder = folder,
            PublicId = publicId,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }
    
     public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var publicId = ExtractPublicIdFromUrl(fileUrl);
            if (string.IsNullOrEmpty(publicId)) return false;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }

    private static (string? Folder, string PublicId) GetFolderAndPublicId(string fullPath)
    {
        fullPath = fullPath.Replace("\\", "/");
        var folder = Path.GetDirectoryName(fullPath)?.Replace("\\", "/");
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fullPath);
        return (folder, fileNameWithoutExt);
    }

    private static string ExtractPublicIdFromUrl(string url)
    {
        try 
        {
            var uri = new Uri(url);
            var pathSegments = uri.Segments; 
            int uploadIndex = -1;
            for(int i=0; i < pathSegments.Length; i++)
            {
                if(pathSegments[i].Contains("upload")) 
                {
                    uploadIndex = i;
                    break;
                }
            }
            if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length) return string.Empty;
            var publicIdParts = pathSegments.Skip(uploadIndex + 2).ToList();
            var publicIdWithExt = string.Join("", publicIdParts);
            var lastDotIndex = publicIdWithExt.LastIndexOf('.');
            if (lastDotIndex > 0) return publicIdWithExt.Substring(0, lastDotIndex);
            return publicIdWithExt;
        }
        catch { return string.Empty; }
    }
    
    public async Task<byte[]> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetByteArrayAsync(fileUrl, cancellationToken);
    }

    public async Task<List<string>> UploadMultipleAsync(IEnumerable<IFormFile> files, string folderPath, CancellationToken cancellationToken = default)
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