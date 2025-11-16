namespace FitTrackPro.Application.Features.Users.Commands.Register;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Users.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            return Result<AuthResponseDto>.Failure("User with this email already exists");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Save refresh token
        user.UpdateRefreshToken(refreshToken, refreshTokenExpiry);
        await _context.SaveChangesAsync(cancellationToken);

        // Send welcome email (fire and forget)
        // _ = Task.Run(() => _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName), cancellationToken);
        _ = _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

        // Return response
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