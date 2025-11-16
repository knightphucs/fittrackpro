namespace FitTrackPro.Application.Features.Users.DTOs;

public class AuthResponseDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
}