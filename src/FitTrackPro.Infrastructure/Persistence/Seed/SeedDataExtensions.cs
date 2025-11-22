namespace FitTrackPro.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class SeedDataExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

            var seeder = new DatabaseSeeder(context, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}
