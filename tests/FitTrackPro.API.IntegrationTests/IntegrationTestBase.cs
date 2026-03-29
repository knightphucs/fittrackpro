namespace FitTrackPro.API.IntegrationTests;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FitTrackPro.Application.Features.Users.Commands.Register;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Constants;
using Microsoft.EntityFrameworkCore;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        // Clean database before each test
        ResetDatabase();
    }

    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Ensure required roles exist for tests
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (!roleManager.RoleExistsAsync(Roles.User).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole<Guid>(Roles.User)).GetAwaiter().GetResult();
        }
        if (!roleManager.RoleExistsAsync(Roles.Administrator).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Administrator)).GetAwaiter().GetResult();
        }
    }

    protected async Task<AuthResponseDto> RegisterAndLoginUserAsync(
        string email = "test@example.com",
        string password = "Password123!",
        string firstName = "Test",
        string lastName = "User")
    {
        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!;
    }

    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<Food> SeedFoodAsync(string name = "Test Food", int calories = 100)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var macros = new MacroNutrients(10m, 5m, 2m);

        var food = Food.Create(
            name: name,
            nameVi: null,
            category: "Test Category",
            servingSize: 100,
            servingUnit: "g",
            calories: calories,
            macros: macros,
            isUserCreated: false
        );

        context.Foods.Add(food);
        await context.SaveChangesAsync();

        return food;
    }

    protected async Task ConfirmUserEmailAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.SingleAsync(u => u.Email == email);
        user.EmailConfirmed = true;
        await context.SaveChangesAsync();
    }
}
