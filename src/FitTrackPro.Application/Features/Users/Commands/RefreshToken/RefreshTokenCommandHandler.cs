namespace FitTrackPro.Application.Features.Users.Commands.RefreshToken;

using MediatR;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        UserManager<User> userManager,
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid access token.");
        }

        // 2. Lấy Jti (Token ID) từ Access Token cũ
        var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

        var storedToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid token.");
        }

        // Check hết hạn
        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure("Token has expired. Please login again.");
        }

        if (storedToken.Invalidated)
        {
            return Result<AuthResponseDto>.Failure("Token has been invalidated.");
        }

        if (storedToken.Used)
        {
            storedToken.Invalidated = true;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<AuthResponseDto>.Failure("Security alert: Token reused. All sessions are revoked.");
        }

        storedToken.Used = true;
        
        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            JwtId = Guid.NewGuid().ToString(),
            UserId = user.Id,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Used = false,
            Invalidated = false
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = newRefreshTokenEntity.ExpiryDate
        });
    }
}