namespace FitTrackPro.Application.Tests.Features.MealLogs;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using FitTrackPro.Application.Features.MealLogs.Commands.LogMeal;
using FitTrackPro.Application.Tests.Persistence;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.ValueObjects;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Repositories;

public class LogMealCommandHandlerTests
{
    private FakeInMemoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FakeInMemoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FakeInMemoryDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogMeal()
    {
        // Arrange
        var context = CreateContext();
        var cache = new Mock<ICacheService>();
        var repoMock = new Mock<IMealLogRepository>();

        var food = Food.Create(
            "Phở Bò",
            "Phở Bò",
            "Breakfast",
            500,
            "bowl",
            450,
            new MacroNutrients(25, 60, 12)
        );

        context.Foods.Add(food);
        await context.SaveChangesAsync();

        var handler = new LogMealCommandHandler(context, cache.Object, repoMock.Object);

        var command = new LogMealCommand
        {
            UserId = Guid.NewGuid(),
            FoodId = food.Id,
            MealType = MealType.Breakfast,
            ServingMultiplier = 1.5m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCalories.Should().Be((int)(450 * 1.5));

        context.MealLogs.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentFood_ShouldReturnFailure()
    {
        // Arrange
        var context = CreateContext();
        var cache = new Mock<ICacheService>();
        var repoMock = new Mock<IMealLogRepository>();

        var handler = new LogMealCommandHandler(context, cache.Object, repoMock.Object);

        var command = new LogMealCommand
        {
            UserId = Guid.NewGuid(),
            FoodId = Guid.NewGuid(), // does not exist
            MealType = MealType.Breakfast,
            ServingMultiplier = 1m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Food item not found.");
    }
}
