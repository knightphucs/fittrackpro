namespace FitTrackPro.API.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Progress.Commands.LogWeight;
using FitTrackPro.Application.Features.Progress.DTOs;

public class ProgressControllerTests : IntegrationTestBase
{
    public ProgressControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task LogWeight_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var command = new LogWeightCommand
        {
            Weight = 75.5m,
            Notes = "Test weight"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/progress/weight", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ProgressEntryDto>();
        result.Should().NotBeNull();
        result!.Weight.Should().Be(75.5m);
    }

    [Fact]
    public async Task GetProgressHistory_ShouldReturnEntries()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Log some weights
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 80m });
        await Task.Delay(100);
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 79m });
        await Task.Delay(100);
        await Client.PostAsJsonAsync("/api/progress/weight", new LogWeightCommand { Weight = 78m });

        // Act
        var response = await Client.GetAsync("/api/progress/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ProgressEntryDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetStatistics_WithData_ShouldReturnStats()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);

        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        await SetupCompleteProfile();

        // Log multiple weights over time
        for (int i = 0; i < 10; i++)
        {
            await Client.PostAsJsonAsync("/api/progress/weight", 
                new LogWeightCommand { Weight = 80m - i * 0.5m });
            await Task.Delay(100);
        }

        // Act
        var response = await Client.GetAsync("/api/progress/statistics?days=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ProgressStatisticsDto>();
        result.Should().NotBeNull();
        result!.TotalEntries.Should().BeGreaterThan(0);
        result.Trend.Should().NotBeNullOrEmpty();
    }

    private Task SetupCompleteProfile()
    {
        // Placeholder: create a complete user profile here if your API requires it for statistics.
        // Keep as no-op to satisfy compilation; extend with API calls to set up profile data if needed.
        return Task.CompletedTask;
    }
}