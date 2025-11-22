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

public class RegisterCommandHandlerTests : TestBase
{
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        SetupDefaultMocks();

        // Provide a mock DbSet<User> so Context.Users is not null and supports queries/adds.
        var usersList = new List<User>();
        var usersQueryable = usersList.AsQueryable();

        var usersDbSet = new Mock<DbSet<User>>();

        // Support synchronous LINQ operations
        usersDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<User>(usersQueryable.Provider));
        usersDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersQueryable.Expression);
        usersDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersQueryable.ElementType);
        usersDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => usersQueryable.GetEnumerator());

        // Support async enumeration (used by EF Core async query extensions)
        usersDbSet.As<IAsyncEnumerable<User>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<User>(usersQueryable.GetEnumerator()));

        // Capture Adds into the backing list
        usersDbSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => usersList.Add(u));

        ContextMock.SetupGet(c => c.Users).Returns(usersDbSet.Object);

        // Ensure SaveChangesAsync is handled by the mock to avoid NotImplementedException
        ContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new RegisterCommandHandler(
            ContextMock.Object,
            PasswordHasherMock.Object,
            JwtTokenGeneratorMock.Object,
            EmailServiceMock.Object);
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

        ContextMock.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
        ContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        PasswordHasherMock.Verify(x => x.HashPassword("Password123!"), Times.Once);
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
        PasswordHasherMock.Verify(x => x.HashPassword("Password123!"), Times.Once);
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
        JwtTokenGeneratorMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Once);
        JwtTokenGeneratorMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendWelcomeEmail()
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

        // Wait a bit for fire-and-forget task
        await Task.Delay(100);

        // Assert
        EmailServiceMock.Verify(
            x => x.SendWelcomeEmailAsync("test@example.com", "John"),
            Times.Once);
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
        return _inner.Execute(expression);
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
            var taskFromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(innerResultType);
            return (TResult)taskFromResultMethod.Invoke(null, new[] { executionResult });
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
