using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class RelationshipLoadingTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Fact]
    public void Item_GetById_Loads_Author_And_Place()
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        Seed(graph.Entities.Concat(other.Entities).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();

        // Act
        var item = repository.GetById(graph.Item.Id);

        // Assert
        Assert.NotNull(item);
        Assert.NotNull(item.Author);
        Assert.NotNull(item.Place);
        Assert.Equal(graph.User.Id, item.Author.Id);
        Assert.Equal(graph.User.Name, item.Author.Name);
        Assert.Equal(graph.Place.Id, item.Place.Id);
        Assert.Equal(graph.Place.Name, item.Place.Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Place_GetById_Loads_Author_And_Only_Its_Items(bool hasItems)
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        Seed(graph.User, graph.Place);
        Seed(other.Entities);
        if (hasItems)
        {
            Seed(graph.Item);
        }
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPlaceRepository>();

        // Act
        var place = repository.GetById(graph.Place.Id);

        // Assert
        Assert.NotNull(place);
        Assert.NotNull(place.Author);
        Assert.Equal(graph.User.Id, place.Author.Id);
        if (hasItems)
        {
            var item = Assert.Single(place.Items);
            Assert.Equal(graph.Item.Id, item.Id);
            Assert.Equal(graph.Item.Name, item.Name);
        }
        else
        {
            Assert.Empty(place.Items);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShoppingList_GetById_Loads_Author_Entries_And_Their_Items(bool hasEntries)
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        Seed(graph.User, graph.Place, graph.Item, graph.ShoppingList);
        Seed(other.Entities);
        if (hasEntries)
        {
            Seed(graph.Entry);
        }
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShoppingListRepository>();

        // Act
        var list = repository.GetById(graph.ShoppingList.Id);

        // Assert
        Assert.NotNull(list);
        Assert.NotNull(list.Author);
        Assert.Equal(graph.User.Id, list.Author.Id);
        if (hasEntries)
        {
            var entry = Assert.Single(list.Items);
            Assert.Equal(graph.Entry.Id, entry.Id);
            Assert.NotNull(entry.Item);
            Assert.Equal(graph.Item.Id, entry.Item.Id);
            Assert.Equal(graph.Item.Name, entry.Item.Name);
        }
        else
        {
            Assert.Empty(list.Items);
        }
    }
}
