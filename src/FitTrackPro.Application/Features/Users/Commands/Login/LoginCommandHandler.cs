namespace FitTrackPro.Application.Features.Users.Commands.Login;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        UserManager<User> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var minutesLeft = lockoutEnd.HasValue 
                ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes 
                : 5;
            return Result<AuthResponseDto>.Failure($"Account locked. Try again in {minutesLeft} minutes.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {            
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Result<AuthResponseDto>.Failure("Account locked due to failed attempts.");
            }

            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // Check Email Confirmation
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
             return Result<AuthResponseDto>.Failure("Please confirm your email address.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Generate Tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // 6. Save Refresh Token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenString,
            JwtId = Guid.NewGuid().ToString(),
            CreationDate = DateTime.UtcNow,
            ExpiryDate = refreshTokenExpiry,
            Used = false,
            Invalidated = false,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Return Result
        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = refreshTokenExpiry
        });
    }
}
