namespace FitTrackPro.Application.Tests.Common;

using Moq;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Tests.Features.Users.Commands;

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

        JwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access_token");

        JwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");
    }

    public static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();

        // Async support
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        return mockSet;
    }
}
