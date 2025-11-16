namespace FitTrackPro.Application.Common.Interfaces;

using FitTrackPro.Domain.Entities;
using System.Security.Claims;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
