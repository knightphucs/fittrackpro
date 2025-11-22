namespace FitTrackPro.API.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;
using FitTrackPro.Application.Features.MealLogs.DTOs;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Application.Features.Goals.Commands.SetGoal;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Application.Common.Models;

public class MealLogsControllerTests : IntegrationTestBase
{
    public MealLogsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task LogMeal_WithValidData_ShouldReturnOk()
    {
        // Arrange - Register, login, set profile & goal
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupCompleteProfile();

        // Get a food to log
        var food = await SeedFoodAsync("Test Food", 52);

        var command = new LogMealCommand
        {
            FoodId = food.Id,
            MealType = MealType.Breakfast,
            ServingMultiplier = 1.5m,
            Notes = "Test meal"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/meallogs", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MealLogDto>();
        result.Should().NotBeNull();
        result!.FoodId.Should().Be(food.Id);
        result.ServingMultiplier.Should().Be(1.5m);
        result.TotalCalories.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDailyMeals_AfterLogging_ShouldReturnMealsWithSummary()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupCompleteProfile();

        // Log 3 meals
        var food1 = await SeedFoodAsync("Breakfast Item", 300);
        var food2 = await SeedFoodAsync("Lunch Item", 500);
        var food3 = await SeedFoodAsync("Dinner Item", 400);

        var logTime = DateTime.UtcNow;

        LogMealCommand CreateCommand(Guid foodId, MealType type) => new LogMealCommand
        {
            FoodId = foodId,
            MealType = type,
            ServingMultiplier = 1.0m,
            UserId = authResponse.UserId,
            LoggedAt = logTime
        };

        await Client.PostAsJsonAsync("/api/meallogs", CreateCommand(food1.Id, MealType.Breakfast));
        await Client.PostAsJsonAsync("/api/meallogs", CreateCommand(food2.Id, MealType.Lunch));
        await Client.PostAsJsonAsync("/api/meallogs", CreateCommand(food3.Id, MealType.Dinner));

        // Act - Query specifically for the date we logged
        var dateString = logTime.ToString("yyyy-MM-dd");
        var response = await Client.GetAsync($"/api/meallogs/daily?date={dateString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DailyMealsDto>();
        result.Should().NotBeNull();
        result!.Meals.Should().HaveCount(3);
        result.Summary.TotalCalories.Should().BeGreaterThan(0);
        result.Summary.CaloriesRemaining.Should().NotBe(0);
    }

    [Fact]
    public async Task DeleteMealLog_ShouldRemoveMeal()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupCompleteProfile();

        var food = await SeedFoodAsync("Delete Me", 100);

        // Log a meal
        var logResponse = await Client.PostAsJsonAsync("/api/meallogs", new LogMealCommand
        {
            FoodId = food.Id,
            MealType = MealType.Breakfast,
            ServingMultiplier = 1.0m
        });
        logResponse.EnsureSuccessStatusCode();

        var mealLog = await logResponse.Content.ReadFromJsonAsync<MealLogDto>();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/meallogs/{mealLog!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var dailyResponse = await Client.GetAsync("/api/meallogs/daily");
        var daily = await dailyResponse.Content.ReadFromJsonAsync<DailyMealsDto>();
        daily!.Meals.Should().BeEmpty();
    }

    private async Task SetupCompleteProfile()
    {
        await Client.PutAsJsonAsync("/api/users/profile", new UpdateProfileCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = Gender.Male,
            Height = 175m
        });

        await Client.PostAsJsonAsync("/api/goals", new SetGoalCommand
        {
            CurrentWeight = 80m,
            TargetWeight = 75m,
            TargetDate = DateTime.Today.AddMonths(3),
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        });
    }
}
