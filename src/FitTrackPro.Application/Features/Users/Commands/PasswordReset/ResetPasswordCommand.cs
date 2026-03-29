using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Users.Commands.PasswordReset
{
    public record ResetPasswordCommand(
        string Email,
        string Token,
        string NewPassword
    ) : IRequest<Result<string>>;
}
