using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Users.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(string Email, string Token) : IRequest<Result<string>>;
}
