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
}