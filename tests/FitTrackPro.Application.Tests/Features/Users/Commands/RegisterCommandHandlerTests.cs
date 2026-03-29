namespace FitTrackPro.Application.Tests.Features.Users.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FitTrackPro.Application.Features.Users.Commands.Register;
using FitTrackPro.Application.Tests.Common;
using FitTrackPro.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Identity;

public class RegisterCommandHandlerTests : TestBase
{
    private readonly RegisterCommandHandler _handler;
    private readonly Mock<UserManager<User>> _userManagerMock;

    public RegisterCommandHandlerTests()
    {
        SetupDefaultMocks();

        // Mock UserManager<User>
        var userStoreMock = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Setup UserManager behaviors
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null); // No existing user

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" });

        // Provide a mock DbSet<RefreshToken>
        var refreshTokensList = new List<RefreshToken>();
        var refreshTokensQueryable = refreshTokensList.AsQueryable();

        var refreshTokensDbSet = new Mock<DbSet<RefreshToken>>();
        refreshTokensDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<RefreshToken>(refreshTokensQueryable.Provider));
        refreshTokensDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.Expression).Returns(refreshTokensQueryable.Expression);
        refreshTokensDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.ElementType).Returns(refreshTokensQueryable.ElementType);
        refreshTokensDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.GetEnumerator()).Returns(() => refreshTokensQueryable.GetEnumerator());
        refreshTokensDbSet.As<IAsyncEnumerable<RefreshToken>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<RefreshToken>(refreshTokensQueryable.GetEnumerator()));
        refreshTokensDbSet.Setup(d => d.Add(It.IsAny<RefreshToken>())).Callback<RefreshToken>(t => refreshTokensList.Add(t));

        ContextMock.SetupGet(c => c.RefreshTokens).Returns(refreshTokensDbSet.Object);
        ContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new RegisterCommandHandler(
            _userManagerMock.Object,
            ContextMock.Object,
            JwtTokenGeneratorMock.Object);
    }

    protected new void SetupDefaultMocks()
    {
        base.SetupDefaultMocks();
        
        // Update GenerateAccessToken to accept roles parameter
        JwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access_token");

        JwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), "Password123!"), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
        ContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), "Password123!"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokens()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        JwtTokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Once);
        JwtTokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = User.Create("test@example.com", "Existing", "User");
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User with this email already exists");
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}

// Helper types to support async LINQ operations on mocked DbSet<T>
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        var result = _inner.Execute(expression);
        return result!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    // For the IAsyncQueryProvider in this EF Core version ExecuteAsync returns TResult (often Task<T>)
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult);

        // If EF expects a Task<T> (most common), wrap the synchronous execution result into Task.FromResult<T>
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerResultType = resultType.GetGenericArguments()[0];

            // Call IQueryProvider.Execute<innerResultType>(expression) via reflection.
            // Find the generic Execute<TResult>(Expression) definition to avoid ambiguous matches.
            var executeMethodDefinition = typeof(IQueryProvider).GetMethods()
                .Where(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                .Single();
            var executeMethod = executeMethodDefinition.MakeGenericMethod(innerResultType);
            var executionResult = executeMethod.Invoke(_inner, new object[] { expression });

            // Return Task.FromResult((innerResultType)executionResult) cast to TResult
            var taskFromResultMethodInfo = typeof(Task).GetMethod(nameof(Task.FromResult), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (taskFromResultMethodInfo == null)
            {
                throw new InvalidOperationException("Unable to find Task.FromResult method via reflection.");
            }
            var taskFromResultMethod = taskFromResultMethodInfo.MakeGenericMethod(innerResultType);
            var taskObj = taskFromResultMethod.Invoke(null, [executionResult]) ?? throw new InvalidOperationException("Task.FromResult returned null.");
            return (TResult)taskObj;
        }

        // Otherwise fall back to sync Execute<TResult>
        return _inner.Execute<TResult>(expression);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }
}
