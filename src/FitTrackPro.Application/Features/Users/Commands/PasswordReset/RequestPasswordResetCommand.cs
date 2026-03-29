using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Users.Commands.PasswordReset
{
    public record RequestPasswordResetCommand(string Email) : IRequest<Result<string>>;
}
