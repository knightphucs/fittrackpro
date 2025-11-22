namespace FitTrackPro.API.IntegrationTests.Controllers;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Goals.Commands.SetGoal;
using FitTrackPro.Application.Features.Goals.DTOs;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Domain.Enums;

public class GoalsControllerTests : IntegrationTestBase
{
    public GoalsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task SetGoal_WithCompleteProfile_ShouldReturnOkWithCalculations()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        // Pass the actual user ID to FakeAuth
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Complete profile first
        var updateProfileCommand = new UpdateProfileCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = Gender.Male,
            Height = 175m
        };
        await Client.PutAsJsonAsync("/api/users/profile", updateProfileCommand);

        var setGoalCommand = new SetGoalCommand
        {
            CurrentWeight = 80m,
            TargetWeight = 75m,
            TargetDate = DateTime.Today.AddMonths(3),
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/goals", setGoalCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var goal = await response.Content.ReadFromJsonAsync<UserGoalDto>();
        goal.Should().NotBeNull();
        goal!.CurrentWeight.Should().Be(80m);
        goal.TargetWeight.Should().Be(75m);
        goal.DailyCalories.Should().BeGreaterThan(0);
        goal.TargetProtein.Should().BeGreaterThan(0);
        goal.TargetCarbs.Should().BeGreaterThan(0);
        goal.TargetFat.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SetGoal_WithoutCompleteProfile_ShouldReturnBadRequest()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        // Pass the actual user ID to FakeAuth
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Don't update profile (missing required fields)
        var setGoalCommand = new SetGoalCommand
        {
            CurrentWeight = 80m,
            TargetWeight = 75m,
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/goals", setGoalCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
