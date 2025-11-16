namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Events;

public class User : BaseEntity, IAuditableEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public decimal? Height { get; private set; } // cm
    public string? ProfilePhotoUrl { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public UserGoal? CurrentGoal { get; private set; }
    public ICollection<MealLog> MealLogs { get; private set; } = new List<MealLog>();
    public ICollection<ProgressEntry> ProgressEntries { get; private set; } = new List<ProgressEntry>();

    private User() { } // EF Core

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));

        return user;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        DateTime? dateOfBirth,
        Gender? gender,
        decimal? height)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Height = height;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public int? GetAge()
    {
        if (!DateOfBirth.HasValue) return null;

        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;

        return age;
    }
}