using FitTrackPro.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FitTrackPro.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // Database
        healthChecks.AddDbContextCheck<ApplicationDbContext>(
            name: "Database",
            failureStatus: HealthStatus.Unhealthy);

        // Redis (optional)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            healthChecks.AddRedis(
                redisConnection,
                name: "Redis",
                failureStatus: HealthStatus.Degraded);
        }

        return services;
    }
}
