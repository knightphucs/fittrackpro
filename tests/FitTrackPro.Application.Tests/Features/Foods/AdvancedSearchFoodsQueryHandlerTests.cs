// File: FitTrackPro.Application.Tests.Features.Foods.SearchFoodsQueryHandlerTests.cs
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using FitTrackPro.Application.Features.Foods.Queries.SearchFoods;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Application.Common.Models;

namespace FitTrackPro.Application.Tests.Features.Foods;

public class AdvancedSearchFoodsQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<AdvancedSearchFoodsQueryHandler>> _loggerMock;
    private readonly AdvancedSearchFoodsQueryHandler _handler;

    public AdvancedSearchFoodsQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<AdvancedSearchFoodsQueryHandler>>();
        _handler = new AdvancedSearchFoodsQueryHandler(_searchServiceMock.Object, _cacheServiceMock.Object, _contextMock.Object, _currentUserServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_SearchService_When_Cache_Miss()
    {
        // Arrange
        var query = new AdvancedSearchFoodsQuery 
        { 
            SearchTerm = "Phở", 
            PageNumber = 1, 
            PageSize = 10 
        };

        var expectedFoods = new List<FoodDto>
        {
            new FoodDto { Id = Guid.NewGuid(), Name = "Phở Bò", Calories = 450 },
            new FoodDto { Id = Guid.NewGuid(), Name = "Phở Gà", Calories = 400 }
        };

        var expectedResult = new PaginatedList<FoodDto>(expectedFoods, 2, 1, 10);

        _cacheServiceMock.Setup(x => x.GetAsync<PaginatedList<FoodDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginatedList<FoodDto>?)null);

        _searchServiceMock.Setup(x => x.AdvancedSearchFoodsAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.First().Name.Should().Be("Phở Bò");

        // Verify
        _searchServiceMock.Verify(x => x.AdvancedSearchFoodsAsync(
            query,
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify cache set
        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            expectedResult, 
            It.IsAny<TimeSpan>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_Return_From_Cache_When_Hit()
    {
        // Arrange
        var query = new AdvancedSearchFoodsQuery { SearchTerm = "Phở" };
        var cachedData = new PaginatedList<FoodDto>(new List<FoodDto>(), 0, 1, 10);

        _cacheServiceMock.Setup(x => x.GetAsync<PaginatedList<FoodDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify cache set
        _searchServiceMock.Verify(x => x.AdvancedSearchFoodsAsync(
            It.IsAny<AdvancedSearchFoodsQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}