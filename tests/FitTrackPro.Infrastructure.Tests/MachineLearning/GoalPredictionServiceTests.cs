namespace FitTrackPro.Infrastructure.Tests.MachineLearning;

using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using FitTrackPro.Infrastructure.MachineLearning;
using FitTrackPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using MediatR;
using FitTrackPro.Domain.Repositories;

public class GoalPredictionServiceTests
{
    [Fact]
    public async Task PredictGoalAchievement_WithValidData_ShouldReturnPrediction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var publisher = new Mock<IPublisher>();
        var context = new ApplicationDbContext(options, publisher.Object);
        var repoMock = new Mock<IMealLogRepository>();
        var logger = new Mock<ILogger<GoalPredictionService>>();

        var service = new GoalPredictionService(context, logger.Object, repoMock.Object);

        // Create test user with goal
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe");

        user.UpdateProfile("John", "Doe", new DateOnly(1990, 1, 1), 
            Gender.Male, 175m);

        var goal = UserGoal.Create(
            user.Id,
            80m,
            75m,
            DateTime.UtcNow.AddMonths(3),
            ActivityLevel.Moderate,
            WeightGoal.Lose,
            2000,
            new MacroNutrients(150, 200, 65));

        context.Users.Add(user);
        context.UserGoals.Add(goal);

        // Add progress entries
        for (int i = 0; i < 10; i++)
        {
            var entry = ProgressEntry.Create(
                user.Id,
                80m - (i * 0.3m),
                DateTime.UtcNow.AddDays(-i * 3));

            context.ProgressEntries.Add(entry);
        }

        await context.SaveChangesAsync();

        // Act
        var result = await service.PredictGoalAchievementAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.EstimatedDaysToGoal.Should().BeGreaterThan(0);
        result.ConfidenceLevel.Should().BeGreaterThan(0);
        result.Recommendation.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PredictGoalAchievement_WithInsufficientData_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var publisher = new Mock<IPublisher>();
        var context = new ApplicationDbContext(options, publisher.Object);
        var repoMock = new Mock<IMealLogRepository>();
        var logger = new Mock<ILogger<GoalPredictionService>>();

        var service = new GoalPredictionService(context, logger.Object, repoMock.Object);

        var userId = Guid.NewGuid();

        // Act
        var result = await service.PredictGoalAchievementAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}