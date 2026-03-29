using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FitTrackPro.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task MigrateAndSeedDatabaseAsync(
        this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing")) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await db.Database.MigrateAsync();
            await app.SeedDatabaseAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration failed");
        }
    }
}
