namespace FitTrackPro.Application.Features.Users.Commands.Register;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;

public record RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}
