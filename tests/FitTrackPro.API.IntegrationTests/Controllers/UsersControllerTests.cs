namespace FitTrackPro.API.IntegrationTests.Controllers;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Domain.Enums;

public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetProfile_WithValidToken_ShouldReturnUserProfile()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Act
        var response = await Client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("test@example.com");
        profile.FirstName.Should().Be("Test");
        profile.LastName.Should().Be("User");
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var command = new UpdateProfileCommand
        {
            FirstName = "Updated",
            LastName = "Name",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Gender.Male,
            Height = 175m
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/users/profile", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var profileResponse = await Client.GetAsync("/api/users/profile");
        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();

        profile!.FirstName.Should().Be("Updated");
        profile.LastName.Should().Be("Name");
        profile.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        profile.Gender.Should().Be(Gender.Male);
        profile.Height.Should().Be(175m);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidHeight_ShouldReturnBadRequest()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var command = new UpdateProfileCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Height = -10m // Invalid height
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/users/profile", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}