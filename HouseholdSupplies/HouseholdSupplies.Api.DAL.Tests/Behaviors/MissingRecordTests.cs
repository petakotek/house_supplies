using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Api.DAL.EF;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class MissingRecordTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    [InlineData("ShoppingListItem")]
    public void GetById_Returns_Null_For_Empty_Guid_In_Populated_Database(string entityType)
    {
        // Arrange
        Seed(new TestDataGraph().Entities);
        using var scope = CreateScope();
        var services = scope.ServiceProvider;
        var resolvedUserRepository = services.GetRequiredService<IUserRepository>();
        var resolvedPlaceRepository = services.GetRequiredService<IPlaceRepository>();
        var resolvedItemRepository = services.GetRequiredService<IItemRepository>();
        var resolvedShoppingListRepository = services.GetRequiredService<IShoppingListRepository>();
        var resolvedShoppingListItemRepository = services.GetRequiredService<IShoppingListItemRepository>();

        // Act
        EntityBase? result = entityType switch
        {
            "User" => resolvedUserRepository.GetById(Guid.Empty),
            "Place" => resolvedPlaceRepository.GetById(Guid.Empty),
            "Item" => resolvedItemRepository.GetById(Guid.Empty),
            "ShoppingList" => resolvedShoppingListRepository.GetById(Guid.Empty),
            "ShoppingListItem" => resolvedShoppingListItemRepository.GetById(Guid.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    [InlineData("ShoppingListItem")]
    public void Update_Missing_Entity_Returns_Null_And_Preserves_Existing_Data(string entityType)
    {
        // Arrange
        var existing = new TestDataGraph();
        var missing = new TestDataGraph();
        var missingUser = missing.User with { Id = Guid.Empty };
        var missingPlace = missing.Place with { Id = Guid.Empty };
        var missingItem = missing.Item with { Id = Guid.Empty };
        var missingList = missing.ShoppingList with { Id = Guid.Empty };
        var missingEntry = missing.Entry with { Id = Guid.Empty };
        Seed(existing.Entities);
        using var scope = CreateScope();
        using var verification = CreateDbContext();
        var services = scope.ServiceProvider;
        var resolvedUserRepository = services.GetRequiredService<IUserRepository>();
        var resolvedPlaceRepository = services.GetRequiredService<IPlaceRepository>();
        var resolvedItemRepository = services.GetRequiredService<IItemRepository>();
        var resolvedShoppingListRepository = services.GetRequiredService<IShoppingListRepository>();
        var resolvedShoppingListItemRepository = services.GetRequiredService<IShoppingListItemRepository>();

        // Act
        var updatedId = entityType switch
        {
            "User" => resolvedUserRepository.Update(missingUser),
            "Place" => resolvedPlaceRepository.Update(missingPlace),
            "Item" => resolvedItemRepository.Update(missingItem),
            "ShoppingList" => resolvedShoppingListRepository.Update(missingList),
            "ShoppingListItem" => resolvedShoppingListItemRepository.Update(missingEntry),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        // Assert
        Assert.Null(updatedId);
        AssertGraphRemains(existing, verification);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    [InlineData("ShoppingListItem")]
    public void Remove_Empty_Guid_Does_Not_Change_Existing_Data(string entityType)
    {
        // Arrange
        var graph = new TestDataGraph();
        Seed(graph.Entities);
        using var scope = CreateScope();
        using var verification = CreateDbContext();
        var services = scope.ServiceProvider;
        var resolvedUserRepository = services.GetRequiredService<IUserRepository>();
        var resolvedPlaceRepository = services.GetRequiredService<IPlaceRepository>();
        var resolvedItemRepository = services.GetRequiredService<IItemRepository>();
        var resolvedShoppingListRepository = services.GetRequiredService<IShoppingListRepository>();
        var resolvedShoppingListItemRepository = services.GetRequiredService<IShoppingListItemRepository>();
        Action remove = entityType switch
        {
            "User" => () => resolvedUserRepository.Remove(Guid.Empty),
            "Place" => () => resolvedPlaceRepository.Remove(Guid.Empty),
            "Item" => () => resolvedItemRepository.Remove(Guid.Empty),
            "ShoppingList" => () => resolvedShoppingListRepository.Remove(Guid.Empty),
            "ShoppingListItem" => () => resolvedShoppingListItemRepository.Remove(Guid.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        // Act
        var exception = Record.Exception(remove);

        // Assert
        Assert.Null(exception);
        AssertGraphRemains(graph, verification);
    }

    private static void AssertGraphRemains(TestDataGraph graph, HouseholdSuppliesDbContext context)
    {
        Assert.Equal(graph.User.Id, Assert.Single(context.Users.ToList()).Id);
        Assert.Equal(graph.Place.Id, Assert.Single(context.Places.ToList()).Id);
        Assert.Equal(graph.Item.Id, Assert.Single(context.Items.ToList()).Id);
        Assert.Equal(graph.ShoppingList.Id, Assert.Single(context.ShoppingLists.ToList()).Id);
        Assert.Equal(graph.Entry.Id, Assert.Single(context.ShoppingListItems.ToList()).Id);
    }
}
