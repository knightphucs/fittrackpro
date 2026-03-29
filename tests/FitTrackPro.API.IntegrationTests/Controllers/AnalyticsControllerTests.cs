namespace FitTrackPro.API.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Analytics.DTOs;
using FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;
using FitTrackPro.Application.Features.Progress.Commands.LogWeight;
using FitTrackPro.Domain.Enums;

public class AnalyticsControllerTests : IntegrationTestBase
{
    public AnalyticsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetWeeklyReport_WithData_ShouldReturnReport()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupUserData();

        // Act
        var response = await Client.GetAsync("/api/analytics/weekly-report");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var report = await response.Content.ReadFromJsonAsync<WeeklyReportDto>();
        report.Should().NotBeNull();
        report!.TotalDays.Should().Be(7);
        report.Achievements.Should().NotBeEmpty();
        report.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNutritionTrends_WithData_ShouldReturnTrends()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupUserData();

        // Act
        var response = await Client.GetAsync("/api/analytics/nutrition-trends?days=7");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var trends = await response.Content.ReadFromJsonAsync<NutritionTrendsDto>();
        trends.Should().NotBeNull();
        trends!.DailyData.Should().NotBeEmpty();
        trends.MacroTrends.Should().NotBeNull();
        trends.CalorieTrend.Should().NotBeNull();
        trends.TopFoods.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExportData_Progress_ShouldReturnCsv()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Log some progress
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 75m });
        await Task.Delay(100);
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 74.5m });

        // Act
        var response = await Client.GetAsync("/api/analytics/export?type=progress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().Contain("Date,Weight");
        csv.Should().Contain("75");
        csv.Should().Contain("74.5");
    }

    [Fact]
    public async Task ExportData_MealLogs_ShouldReturnCsv()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Log some meals
        var food = await SeedFoodAsync("Test Food", 200);
        await Client.PostAsJsonAsync("/api/meallogs", new LogMealCommand
        {
            FoodId = food.Id,
            MealType = MealType.Breakfast,
            ServingMultiplier = 1.0m
        });

        // Act
        var response = await Client.GetAsync("/api/analytics/export?type=meallogs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().Contain("Date,Time,Meal Type");
        csv.Should().Contain("Test Food");
    }

    [Fact]
    public async Task GetWeeklyReport_Cached_ShouldReturnFast()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupUserData();

        // First call (cache miss)
        var startTime1 = DateTime.UtcNow;
        await Client.GetAsync("/api/analytics/weekly-report");
        var duration1 = (DateTime.UtcNow - startTime1).TotalMilliseconds;

        // Second call (cache hit)
        var startTime2 = DateTime.UtcNow;
        var response = await Client.GetAsync("/api/analytics/weekly-report");
        var duration2 = (DateTime.UtcNow - startTime2).TotalMilliseconds;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second call should be faster (cached)
        duration2.Should().BeLessThan(duration1);
    }

    private async Task SetupUserData()
    {
        // Setup profile
        await Client.PutAsJsonAsync("/api/users/profile", new
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = Gender.Male,
            Height = 175m
        });

        // Set goal
        await Client.PostAsJsonAsync("/api/goals", new
        {
            CurrentWeight = 80m,
            TargetWeight = 75m,
            TargetDate = DateTime.Today.AddMonths(3),
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        });

        // Log some meals over the past week
        for (int i = 0; i < 7; i++)
        {
            var food = await SeedFoodAsync($"Food Day {i}", 300 + (i * 50));
            
            await Client.PostAsJsonAsync("/api/meallogs", new LogMealCommand
            {
                FoodId = food.Id,
                MealType = MealType.Breakfast,
                ServingMultiplier = 1.0m,
                LoggedAt = DateTime.UtcNow.AddDays(-i)
            });

            await Task.Delay(50);
        }

        // Log some weights
        for (int i = 0; i < 7; i++)
        {
            await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand
            {
                Weight = 80m - (i * 0.2m),
                RecordedAt = DateTime.UtcNow.AddDays(-i)
            });

            await Task.Delay(50);
        }
    }
}