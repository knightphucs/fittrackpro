using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;

namespace FitTrackPro.API.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string hangfireConnectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("HangfireConnection connection string is not configured.");

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireConnectionString));
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.ServerName = $"fittrackpro-{Environment.MachineName}";
        });

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboardWithAuth(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter(env) },
            DashboardTitle = "FitTrackPro Hangfire Dashboard"
        });

        return app;
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireAuthorizationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool Authorize(DashboardContext context)
        => _env.IsDevelopment();
}
