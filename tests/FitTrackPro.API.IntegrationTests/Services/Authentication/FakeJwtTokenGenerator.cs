namespace FitTrackPro.API.IntegrationTests.Services.Authentication;

using System.Collections.Generic;
using System.Security.Claims;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateAccessToken(User user, IEnumerable<string> roles) => "test-access-token-with-roles";

    public string GenerateRefreshToken() => "test-refresh-token";
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) => null;
}
