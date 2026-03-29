namespace FitTrackPro.Domain.Entities;

using System.Security.Cryptography;
using FitTrackPro.Domain.Common;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Events;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser<Guid>, IAuditableEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) =>_domainEvents.Add(domainEvent);

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateOnly? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public decimal? Height { get; private set; } // cm
    public string? ProfilePhotoUrl { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public UserGoal? CurrentGoal { get; private set; }
    public ICollection<ProgressEntry> ProgressEntries { get; private set; } = new List<ProgressEntry>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { } // EF Core

    public static User Create(
        string email,
        string firstName,
        string lastName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));

        return user;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        Gender? gender,
        decimal? height)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Height = height;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = Id.ToString();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public int? GetAge()
    {
        if (!DateOfBirth.HasValue) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value > today.AddYears(-age)) age--;

        return age;
    }
}
