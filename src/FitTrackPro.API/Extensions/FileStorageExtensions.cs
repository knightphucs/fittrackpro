using Microsoft.Extensions.FileProviders;
using Serilog;

namespace FitTrackPro.API.Extensions;

public static class FileStorageExtensions
{
    public static IApplicationBuilder UseFileStorage(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";

        return storageProvider switch
        {
            "Local" => app.UseLocalFileStorage(configuration),
            // "S3" => app.UseS3FileStorage(configuration),
            // "Azure" => app.UseAzureBlobStorage(configuration),
            _ => throw new InvalidOperationException(
                $"Unsupported FileStorage provider: {storageProvider}")
        };
    }

    private static IApplicationBuilder UseLocalFileStorage(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var localPathConfig = configuration["FileStorage:LocalPath"];

        var uploadsPath = string.IsNullOrWhiteSpace(localPathConfig)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : Path.IsPathRooted(localPathConfig)
                ? localPathConfig
                : Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(), localPathConfig));

        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            Log.Information("Created uploads directory at: {UploadsPath}", uploadsPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append(
                    "Cache-Control", "public,max-age=2592000"); // 30 days
            }
        });

        return app;
    }
}
