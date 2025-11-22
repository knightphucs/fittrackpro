namespace FitTrackPro.Application.Features.Users.DTOs;

using FitTrackPro.Domain.Enums;

public class UserProfileDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public DateOnly? DateOfBirth { get; init; }
    public int? Age { get; init; }
    public Gender? Gender { get; init; }
    public decimal? Height { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public bool HasActiveGoal { get; init; }
}
