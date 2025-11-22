using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FluentAssertions;
using Moq;

namespace FitTrackPro.Application.Tests.Features.Users.Commands;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateProfileCommandHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateUserProfile()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            "test@example.com",
            "hashedpassword",
            "John",
            "Doe"
        );

        _dbContextMock.Setup(x => x.Users.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = Gender.Female,
            Height = 165m,
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        _dbContextMock.Setup(x => x.Users.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateProfileCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }
}
