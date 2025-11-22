namespace FitTrackPro.Domain.Tests.Entities;

using FluentAssertions;
using Xunit;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Enums;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange & Act
        var user = User.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.IsEmailConfirmed.Should().BeFalse();
        user.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void GetFullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData(1990, 1, 1, 35)]
    [InlineData(2000, 6, 15, 25)]
    [InlineData(1985, 12, 31, 39)]
    public void GetAge_WithValidDateOfBirth_ShouldCalculateCorrectAge(
        int year,
        int month,
        int day,
        int expectedAge)
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        user.UpdateProfile(
            "John",
            "Doe",
            new DateOnly(year, month, day),
            Gender.Male,
            175m);

        // Act
        var age = user.GetAge();

        // Assert
        age.Should().Be(expectedAge);
    }

    [Fact]
    public void GetAge_WithNullDateOfBirth_ShouldReturnNull()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        // Act
        var age = user.GetAge();

        // Assert
        age.Should().BeNull();
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateAllFields()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        // Act
        user.UpdateProfile(
            "Jane",
            "Smith",
            new DateOnly(1990, 1, 1),
            Gender.Female,
            165m);

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));
        user.Gender.Should().Be(Gender.Female);
        user.Height.Should().Be(165m);
    }
}
