using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;

namespace FitTrackPro.Application.Features.Users.Commands.PasswordReset
{
    public class RequestPasswordResetCommandHandler 
        : IRequestHandler<RequestPasswordResetCommand, Result<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public RequestPasswordResetCommandHandler(
            UserManager<User> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(
            RequestPasswordResetCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return Result<string>.Success("If your email is registered, you will receive a password reset link shortly.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            var encodedToken = Uri.EscapeDataString(token);

            await _emailService.SendPasswordResetEmailAsync(user.Email!, encodedToken);

            return Result<string>.Success("Password reset link sent.");
        }
    }
}
