namespace FitTrackPro.Application.Features.Users.Commands.Login;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid credentials");
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.UpdateRefreshToken(refreshToken, refreshTokenExpiry);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = refreshTokenExpiry
        };

        return Result<AuthResponseDto>.Success(response);
    }
}