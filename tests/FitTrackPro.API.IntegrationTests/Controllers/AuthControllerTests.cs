namespace FitTrackPro.API.IntegrationTests.Controllers;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Users.Commands.Register;
using FitTrackPro.Application.Features.Users.Commands.Login;
using FitTrackPro.Application.Features.Users.DTOs;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOkWithToken()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Response status: {response.StatusCode}, Content: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email.ToLowerInvariant());
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";

        var firstCommand = new RegisterCommand
        {
            Email = email,
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await Client.PostAsJsonAsync("/api/auth/register", firstCommand);

        var secondCommand = new RegisterCommand
        {
            Email = email,
            Password = "Password456!",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", secondCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")] // Empty email
    [InlineData("invalid-email", "Password123!", "John", "Doe")] // Invalid email format
    [InlineData("test@example.com", "weak", "John", "Doe")] // Weak password
    [InlineData("test@example.com", "Password123!", "", "Doe")] // Empty first name
    [InlineData("test@example.com", "Password123!", "John", "")] // Empty last name
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(
        string email,
        string password,
        string firstName,
        string lastName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange - First register a user
        var email = $"logintest{Guid.NewGuid()}@example.com";
        var password = "Password123!";

        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {registerResponse.StatusCode}, Content: {errorContent}");
        }

        await ConfirmUserEmailAsync(email);

        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(email.ToLowerInvariant());
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginCommand = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange - Register a user
        var email = $"wrongpass{Guid.NewGuid()}@example.com";
        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword123!";

        var registerCommand = new RegisterCommand
        {
            Email = email,
            Password = correctPassword,
            FirstName = "John",
            LastName = "Doe"
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerCommand);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {registerResponse.StatusCode}, Content: {errorContent}");
        }

        await ConfirmUserEmailAsync(email);

        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = wrongPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
