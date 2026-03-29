namespace FitTrackPro.Application.Features.Users.Commands.Register;

using MediatR;
using Microsoft.AspNetCore.Identity; // Cần thiết cho UserManager
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Common;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<AuthResponseDto>.Failure("User with this email already exists");
        }

        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName);

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");
        var roles = await _userManager.GetRolesAsync(user); 

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

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

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = refreshTokenExpiry
        };

        return Result<AuthResponseDto>.Success(response);
    }
}