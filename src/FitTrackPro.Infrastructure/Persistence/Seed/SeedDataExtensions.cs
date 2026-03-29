namespace FitTrackPro.Infrastructure.Persistence.Seed;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var databaseSeederLogger = loggerFactory.CreateLogger<DatabaseSeeder>();
            var databaseSeeder = new DatabaseSeeder(context, databaseSeederLogger, userManager, roleManager);
            await databaseSeeder.SeedAsync();

            var foodLogger = loggerFactory.CreateLogger<FoodSeeder>();
            var foodSeeder = new FoodSeeder(context, foodLogger);
            await foodSeeder.SeedAsync();

            var exerciseLogger = loggerFactory.CreateLogger<ExerciseSeeder>();
            var exerciseSeeder = new ExerciseSeeder(context, exerciseLogger);
            await exerciseSeeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}
