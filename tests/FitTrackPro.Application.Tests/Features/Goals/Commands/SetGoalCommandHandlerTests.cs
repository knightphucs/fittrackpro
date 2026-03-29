using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.Goals.Commands.SetGoal;
using FitTrackPro.Application.Features.Goals.Services;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace FitTrackPro.Application.Tests.Features.Goals.Commands;

public class SetGoalCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly SetGoalCommandHandler _handler;

    public SetGoalCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();

        var userGoalsDbSetMock = new Mock<DbSet<UserGoal>>();
        _dbContextMock.Setup(x => x.UserGoals).Returns(userGoalsDbSetMock.Object);

        _handler = new SetGoalCommandHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetUserGoal()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe"
        );

        typeof(User)
            .GetProperty("Id")!
            .SetValue(user, userId);

        user.UpdateProfile(
            "John",
            "Doe",
            new DateOnly(1990, 1, 1),
            0,
            180m
        );

        _dbContextMock.Setup(x => x.Users.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var age = user.GetAge()!.Value;
        var tdee = TDEECalculator.Calculate(
            user.Gender!.Value,
            age,
            user.Height!.Value,
            80m,
            ActivityLevel.Moderate
        );

        var dailyCalories = TDEECalculator.AdjustForGoal(tdee, WeightGoal.Lose);

        var macros = MacroCalculator.Calculate(
            dailyCalories,
            80m,
            WeightGoal.Lose);

        macros.Should().NotBeNull();

        var goal = UserGoal.Create(
            userId,
            80m,
            70m,
            DateTime.UtcNow.AddMonths(3),
            ActivityLevel.Moderate,
            WeightGoal.Lose,
            dailyCalories,
            macros
        );

        var command = new SetGoalCommand
        {
            UserId = user.Id,
            CurrentWeight = goal.CurrentWeight,
            TargetWeight = goal.TargetWeight,
            TargetDate = goal.TargetDate,
            ActivityLevel = goal.ActivityLevel,
            WeightGoal = goal.WeightGoal
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();

        // Simulate no user found
        _dbContextMock.Setup(x => x.Users.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var command = new SetGoalCommand
        {
            UserId = userId,
            CurrentWeight = 80m,
            ActivityLevel = ActivityLevel.Moderate,
            WeightGoal = WeightGoal.Lose
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found.");
    }
}
