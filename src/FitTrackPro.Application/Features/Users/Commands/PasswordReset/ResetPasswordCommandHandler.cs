using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;

namespace FitTrackPro.Application.Features.Users.Commands.PasswordReset
{
    public class ResetPasswordCommandHandler 
        : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IApplicationDbContext _context;

        public ResetPasswordCommandHandler(UserManager<User> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Result<string>> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Result<string>.Failure("Invalid request.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {                
                var activeTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && !rt.Invalidated)
                    .ToListAsync(cancellationToken);

                // 2. Đánh dấu là đã hủy (Invalidated)
                if (activeTokens.Any())
                {
                    foreach (var token in activeTokens)
                    {
                        token.Invalidated = true;
                    }
                    
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return Result<string>.Success("Password has been reset successfully.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<string>.Failure($"Reset failed: {errors}");
        }
    }
}
