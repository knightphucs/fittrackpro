using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Users.Commands.LogOut;

public record LogoutCommand(string RefreshToken) : IRequest<Result<string>>;