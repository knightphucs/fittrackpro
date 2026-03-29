namespace FitTrackPro.API.IntegrationTests.Features;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using FitTrackPro.Infrastructure.MachineLearning.Models;
using FitTrackPro.Application.Features.Progress.Commands.LogWeight;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Application.Features.Analytics.DTOs;

public class MLPredictionTests : IntegrationTestBase
{
    public MLPredictionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetGoalPrediction_WithSufficientData_ShouldReturnPrediction()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupProfileAndGoal();

        // Log weight progression over time (simulating 10 entries)
        for (int i = 0; i < 10; i++)
        {
            await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand
            {
                Weight = 80m - (i * 0.3m), // Simulating gradual weight loss
                RecordedAt = DateTime.UtcNow.AddDays(-i * 3)
            });
            await Task.Delay(50);
        }

        // Act
        var response = await Client.GetAsync("/api/analytics/goal-prediction");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var prediction = await response.Content.ReadFromJsonAsync<GoalPredictionResult>();
        prediction.Should().NotBeNull();
        prediction!.EstimatedDaysToGoal.Should().BeGreaterThan(0);
        prediction.ConfidenceLevel.Should().BeGreaterThan(0);
        prediction.Recommendation.Should().NotBeNullOrEmpty();
        prediction.EstimatedAchievementDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetGoalPrediction_WithInsufficientData_ShouldReturnError()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupProfileAndGoal();

        // Log only 2 weights (insufficient)
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 80m });
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 79.5m });

        // Act
        var response = await Client.GetAsync("/api/analytics/goal-prediction");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportProgressPdf_ShouldReturnPdfFile()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Log some progress
        for (int i = 0; i < 5; i++)
        {
            await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand
            {
                Weight = 80m - (i * 0.5m)
            });
            await Task.Delay(50);
        }

        // Act
        var response = await Client.GetAsync("/api/analytics/export?type=progress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Length.Should().BeGreaterThan(0);
    }

    private async Task SetupProfileAndGoal()
    {
        await Client.PutAsJsonAsync("/api/users/profile", new
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = Gender.Male,
            Height = 175m
        });

        await Client.PostAsJsonAsync("/api/goals", new
        {
            CurrentWeight = 80m,
            TargetWeight = 75m,
            TargetDate = DateTime.Today.AddMonths(3),
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        });
    }
}