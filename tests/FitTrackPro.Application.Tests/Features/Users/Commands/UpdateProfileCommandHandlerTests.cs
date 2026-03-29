using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.Users.Commands.UpdateProfile;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace FitTrackPro.Application.Tests.Features.Users.Commands;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        _handler = new UpdateProfileCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateUserProfile()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe"
        );

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

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
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));
        user.Gender.Should().Be(Gender.Female);
        user.Height.Should().Be(165m);
        
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
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
        
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe"
        );

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var identityErrors = new[]
        {
            new IdentityError { Description = "Update failed" }
        };
        
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var command = new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update profile");
    }
}
