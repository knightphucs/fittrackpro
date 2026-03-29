namespace FitTrackPro.Application.Features.Users.Commands.RefreshToken;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResponseDto>>;