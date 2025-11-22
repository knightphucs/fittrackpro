namespace FitTrackPro.Application.Features.Users.Commands.Login;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;

public record LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
}
