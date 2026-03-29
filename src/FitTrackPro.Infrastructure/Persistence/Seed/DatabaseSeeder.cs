namespace FitTrackPro.Infrastructure.Persistence.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Constants;
using Microsoft.AspNetCore.Identity;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync();
            }

            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync(Roles.Administrator))
        {
            _logger.LogInformation("Seeding Administrator Role...");
            await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Administrator));
        }

        if (!await _roleManager.RoleExistsAsync(Roles.User))
        {
            _logger.LogInformation("Seeding User Role...");
            await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.User));
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "phuchoang3103@gmail.com";

        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            _logger.LogInformation("Seeding Administrator user...");

            var admin = User.Create(
                adminEmail,
                "Phuc",
                "Gia"
            );

            admin.EmailConfirmed = true;

            admin.ClearDomainEvents();

            var result = await _userManager.CreateAsync(admin, "Admin@123");

            if (result.Succeeded)
            {
                // Gán Role Admin
                await _userManager.AddToRoleAsync(admin, Roles.Administrator);
                _logger.LogInformation("Administrator user seeded successfully.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to seed Admin user: {errors}");
            }
        }
    }
}
