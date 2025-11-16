namespace FitTrackPro.Application.Features.Users.Commands.UpdateProfile;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Domain.Enums;

public record UpdateProfileCommand : IRequest<Result<Unit>>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public decimal? Height { get; init; }
}