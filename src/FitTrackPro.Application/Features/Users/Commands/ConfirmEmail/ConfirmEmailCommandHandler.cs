using MediatR;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Application.Common.Models;

namespace FitTrackPro.Application.Features.Users.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
    {
        private readonly UserManager<User> _userManager;

        public ConfirmEmailCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.Failure("Invalid email.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (result.Succeeded)
            {
                return Result<string>.Success("Email confirmed successfully. You can now login.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<string>.Failure($"Error confirming email: {errors}");
        }
    }
}
