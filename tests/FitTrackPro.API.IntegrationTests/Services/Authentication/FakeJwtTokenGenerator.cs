namespace FitTrackPro.API.IntegrationTests.Services.Authentication;

using System.Security.Claims;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateAccessToken(User user) => "test-access-token";
    public string GenerateRefreshToken() => "test-refresh-token";
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) => null;
}
