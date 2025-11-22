using FluentAssertions;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Features.Foods.Queries.SearchFoods;
using FitTrackPro.Application.Tests.Common;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Tests.Persistence;

namespace FitTrackPro.Application.Tests.Features.Foods;

public class SearchFoodsQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnMatchingFoods()
    {
        var options = new DbContextOptionsBuilder<FakeInMemoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new FakeInMemoryDbContext(options);
        var cache = new Mock<ICacheService>();

        // Arrange
        var foods = new List<Food>
        {
            Food.Create("Phở Bò", "Phở Bò", "Breakfast", 500, "bowl", 450,
                new MacroNutrients(25, 60, 12)),
            Food.Create("Bánh Mì", "Bánh Mì", "Breakfast", 200, "piece", 400,
                new MacroNutrients(18, 45, 15))
        };

        context.Foods.AddRange(foods);
        await context.SaveChangesAsync();

        var handler = new SearchFoodsQueryHandler(context, cache.Object);
        var query = new SearchFoodsQuery { SearchTerm = "Phở" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Phở Bò");
    }
}
