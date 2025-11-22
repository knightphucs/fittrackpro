namespace FitTrackPro.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.Services.Authentication;
using FitTrackPro.Infrastructure.Services.Caching;
using FitTrackPro.Infrastructure.Services.Email;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FitTrackPro_";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
