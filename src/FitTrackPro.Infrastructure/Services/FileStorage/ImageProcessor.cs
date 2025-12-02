namespace FitTrackPro.Infrastructure.Services.FileStorage;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

public static class ImageProcessor
{
    public static async Task<byte[]> CompressImageAsync(
        Stream imageStream,
        int maxWidth = 1920,
        int quality = 85)
    {
        using var image = await Image.LoadAsync(imageStream);
        
        // Resize if too large
        if (image.Width > maxWidth)
        {
            var ratio = (double)maxWidth / image.Width;
            var newHeight = (int)(image.Height * ratio);
            
            image.Mutate(x => x.Resize(maxWidth, newHeight));
        }

        using var outputStream = new MemoryStream();
        var encoder = new JpegEncoder
        {
            Quality = quality
        };

        await image.SaveAsync(outputStream, encoder);
        return outputStream.ToArray();
    }

    public static async Task<(byte[] Original, byte[] Thumbnail)> CreateThumbnailAsync(
        Stream imageStream,
        int thumbnailWidth = 300)
    {
        // Original
        imageStream.Position = 0;
        var originalBytes = new byte[imageStream.Length];
        await imageStream.ReadAsync(originalBytes);

        // Thumbnail
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream);
        
        var ratio = (double)thumbnailWidth / image.Width;
        var thumbnailHeight = (int)(image.Height * ratio);
        
        image.Mutate(x => x.Resize(thumbnailWidth, thumbnailHeight));

        using var thumbnailStream = new MemoryStream();
        await image.SaveAsJpegAsync(thumbnailStream, new JpegEncoder { Quality = 80 });
        
        return (originalBytes, thumbnailStream.ToArray());
    }

    public static async Task<byte[]> AutoRotateAsync(Stream imageStream)
    {
        using var image = await Image.LoadAsync(imageStream);
        
        // Auto-rotate based on EXIF orientation
        image.Mutate(x => x.AutoOrient());

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        
        return outputStream.ToArray();
    }

    // Add watermark to images
    public static async Task<byte[]> AddWatermarkAsync(
        Stream imageStream, 
        string watermarkText)
    {
        using var image = await Image.LoadAsync(imageStream);
        
        var font = SystemFonts.CreateFont("Arial", 24);
        
        image.Mutate(x => x.DrawText(
            watermarkText,
            font,
            Color.White,
            new PointF(10, image.Height - 40)
        ));
        
        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        return outputStream.ToArray();
    }

    public static bool IsValidImageType(string contentType)
    {
        var validTypes = new[] 
        { 
            "image/jpeg", 
            "image/jpg", 
            "image/png", 
            "image/webp" 
        };
        
        return validTypes.Contains(contentType.ToLower());
    }

    public static bool IsValidImageSize(long fileSize, long maxSizeInBytes = 10 * 1024 * 1024)
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }
}