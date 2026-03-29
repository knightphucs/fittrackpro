using Serilog;

namespace FitTrackPro.API.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder UseSerilogLogging(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(configuration["Serilog:WriteTo:0:Args:serverUrl"] ?? "http://localhost:5341")
            .CreateLogger();

        return host.UseSerilog();
    }
}
