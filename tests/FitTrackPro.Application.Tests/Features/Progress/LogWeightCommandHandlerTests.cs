namespace FitTrackPro.Application.Tests.Features.Progress;

using FluentAssertions;
using Xunit;
using Moq;
using FitTrackPro.Application.Features.Progress.Commands.LogWeight;
using FitTrackPro.Application.Tests.Common;
using FitTrackPro.Domain.Entities;

public class LogWeightCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogWeight()
    {
        // Arrange
        var progressData = new List<ProgressEntry>().AsQueryable();
        var progressSetMock = CreateDbSetMock(progressData);
        ContextMock.Setup(x => x.ProgressEntries).Returns(progressSetMock.Object);

        var goalData = new List<UserGoal>().AsQueryable();
        var goalSetMock = CreateDbSetMock(goalData);
        ContextMock.Setup(x => x.UserGoals).Returns(goalSetMock.Object);

        ContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        var handler = new LogWeightCommandHandler(ContextMock.Object, CacheServiceMock.Object);

        var command = new LogWeightCommand
        {
            UserId = Guid.NewGuid(),
            Weight = 75.5m,
            Notes = "Test weight"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Weight.Should().Be(75.5m);
        progressSetMock.Verify(x => x.Add(It.IsAny<ProgressEntry>()), Times.Once);
    }


    [Fact]
    public async Task Handle_WithInvalidWeight_ShouldFail()
    {
        // Validation should catch this
        var validator = new LogWeightCommandValidator();
        var command = new LogWeightCommand
        {
            UserId = Guid.NewGuid(),
            Weight = -10m
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}