namespace FitTrackPro.Application.Tests.Common;

using Moq;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;

public class TestBase
{
    protected Mock<IApplicationDbContext> ContextMock { get; }
    protected Mock<IPasswordHasher> PasswordHasherMock { get; }
    protected Mock<IJwtTokenGenerator> JwtTokenGeneratorMock { get; }
    protected Mock<IEmailService> EmailServiceMock { get; }
    protected Mock<ICacheService> CacheServiceMock { get; }

    public TestBase()
    {
        ContextMock = new Mock<IApplicationDbContext>();
        PasswordHasherMock = new Mock<IPasswordHasher>();
        JwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        EmailServiceMock = new Mock<IEmailService>();
        CacheServiceMock = new Mock<ICacheService>();
    }

    protected void SetupDefaultMocks()
    {
        PasswordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        JwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");

        JwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");
    }
}
