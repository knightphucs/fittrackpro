using Microsoft.AspNetCore.Http.Features;

namespace FitTrackPro.API.Extensions;

public static class FileUploadExtensions
{
    private const long DefaultMaxFileSize = 10 * 1024 * 1024; // 10MB

    public static IServiceCollection ConfigureFileUpload(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var maxFileSize = configuration.GetValue<long?>(
            "FileUpload:MaxFileSize") ?? DefaultMaxFileSize;

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = maxFileSize;
        });

        return services;
    }

    public static IWebHostBuilder ConfigureFileUploadLimits(
        this IWebHostBuilder webHost,
        IConfiguration configuration)
    {
        var maxFileSize = configuration.GetValue<long?>(
            "FileUpload:MaxFileSize") ?? DefaultMaxFileSize;

        webHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = maxFileSize;
        });

        return webHost;
    }
}
