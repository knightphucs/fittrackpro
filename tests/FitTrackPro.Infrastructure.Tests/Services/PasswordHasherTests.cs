namespace FitTrackPro.Infrastructure.Tests.Services;

using FitTrackPro.Infrastructure.Services.Authentication;
using FluentAssertions;
using Xunit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_SamPasswordTwice_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // Due to different salts
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "Password123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "Password123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("SimplePassword")]
    [InlineData("ComplexP@ssw0rd!")]
    [InlineData("Very$Long&Complex#Password123!@#")]
    public void HashAndVerify_WithVariousPasswords_ShouldWork(string password)
    {
        // Act
        var hash = _passwordHasher.HashPassword(password);
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }
}