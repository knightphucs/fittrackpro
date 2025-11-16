namespace FitTrackPro.API.IntegrationTests.Scenarios;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Application.Features.Goals.Commands.SetGoal;
using FitTrackPro.Domain.Enums;

public class CompleteUserJourneyTests : IntegrationTestBase
{
    public CompleteUserJourneyTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteUserJourney_FromRegistrationToGoalSetting_ShouldSucceed()
    {
        // Step 1: Register
        var authResponse = await RegisterAndLoginUserAsync(
            $"journey{Guid.NewGuid()}@example.com",
            "Password123!",
            "John",
            "Doe");

        authResponse.Should().NotBeNull();
        authResponse.AccessToken.Should().NotBeNullOrEmpty();

        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Step 2: Get initial profile
        var profileResponse = await Client.GetAsync("/api/users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Update profile with complete information
        var updateCommand = new UpdateProfileCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 6, 15),
            Gender = Gender.Male,
            Height = 178m
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/users/profile", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4: Set fitness goal
        var goalCommand = new SetGoalCommand
        {
            CurrentWeight = 85m,
            TargetWeight = 78m,
            TargetDate = DateTime.Today.AddMonths(4),
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        };

        var goalResponse = await Client.PostAsJsonAsync("/api/goals", goalCommand);
        goalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify goal was set correctly
        var currentGoalResponse = await Client.GetAsync("/api/goals/current");
        currentGoalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Journey complete!
    }
}