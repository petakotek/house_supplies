using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class UpdateBehaviorTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    [InlineData("ShoppingListItem")]
    public void Detached_Scalar_Update_Preserves_Relationships_And_Other_Graph(string entityType)
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        var user = graph.User with { Name = "Updated", Items = [], Places = [], ShoppingLists = [] };
        var place = graph.Place with { Name = "Updated", Items = [] };
        var item = graph.Item with { Name = "Updated", ShoppingListItems = [] };
        var list = graph.ShoppingList with { Name = "Updated", Items = [] };
        var entry = graph.Entry with { Amount = 4.5, IsBought = true };
        Seed(graph.Entities.Concat(other.Entities).ToArray());
        using var scope = CreateScope();
        var services = scope.ServiceProvider;
        var resolvedUserRepository = services.GetRequiredService<IUserRepository>();
        var resolvedPlaceRepository = services.GetRequiredService<IPlaceRepository>();
        var resolvedItemRepository = services.GetRequiredService<IItemRepository>();
        var resolvedShoppingListRepository = services.GetRequiredService<IShoppingListRepository>();
        var resolvedShoppingListItemRepository = services.GetRequiredService<IShoppingListItemRepository>();
        using var context = CreateDbContext();

        var expectedId = entityType switch
        {
            "User" => graph.User.Id, "Place" => graph.Place.Id, "Item" => graph.Item.Id,
            "ShoppingList" => graph.ShoppingList.Id, "ShoppingListItem" => graph.Entry.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        // Act
        var updatedId = entityType switch
        {
            "User" => resolvedUserRepository.Update(user),
            "Place" => resolvedPlaceRepository.Update(place),
            "Item" => resolvedItemRepository.Update(item),
            "ShoppingList" => resolvedShoppingListRepository.Update(list),
            "ShoppingListItem" => resolvedShoppingListItemRepository.Update(entry),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        // Assert
        var savedUser = context.Users.Single(value => value.Id == graph.User.Id);
        var savedPlace = context.Places.Single(value => value.Id == graph.Place.Id);
        var savedItem = context.Items.Single(value => value.Id == graph.Item.Id);
        var savedList = context.ShoppingLists.Single(value => value.Id == graph.ShoppingList.Id);
        var savedEntry = context.ShoppingListItems.Single(value => value.Id == graph.Entry.Id);
        Assert.Equal(expectedId, updatedId);
        Assert.Equal(entityType == "User" ? "Updated" : graph.User.Name, savedUser.Name);
        Assert.Equal(entityType == "Place" ? "Updated" : graph.Place.Name, savedPlace.Name);
        Assert.Equal(entityType == "Item" ? "Updated" : graph.Item.Name, savedItem.Name);
        Assert.Equal(entityType == "ShoppingList" ? "Updated" : graph.ShoppingList.Name, savedList.Name);
        Assert.Equal(entityType == "ShoppingListItem" ? 4.5 : graph.Entry.Amount, savedEntry.Amount);
        Assert.Equal(entityType == "ShoppingListItem", savedEntry.IsBought);
        Assert.Equal(graph.User.Id, savedPlace.AuthorId);
        Assert.Equal(graph.User.Id, savedItem.AuthorId);
        Assert.Equal(graph.Place.Id, savedItem.PlaceId);
        Assert.Equal(graph.User.Id, savedList.AuthorId);
        Assert.Equal(graph.Item.Id, savedEntry.ItemId);
        Assert.Equal(graph.ShoppingList.Id, savedEntry.ShoppingListId);
        Assert.Equal(2, context.Users.Count());
        Assert.Equal(2, context.Places.Count());
        Assert.Equal(2, context.Items.Count());
        Assert.Equal(2, context.ShoppingLists.Count());
        Assert.Equal(2, context.ShoppingListItems.Count());
        Assert.Equal(other.User.Name, context.Users.Single(value => value.Id == other.User.Id).Name);
        Assert.Equal(other.Place.Name, context.Places.Single(value => value.Id == other.Place.Id).Name);
        Assert.Equal(other.Item.Name, context.Items.Single(value => value.Id == other.Item.Id).Name);
        Assert.Equal(other.ShoppingList.Name, context.ShoppingLists.Single(value => value.Id == other.ShoppingList.Id).Name);
        Assert.Equal(other.Entry.Amount, context.ShoppingListItems.Single(value => value.Id == other.Entry.Id).Amount);
    }

    [Fact]
    public void Item_Update_Can_Change_Author_And_Place_Without_Modifying_Parents()
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        var updated = graph.Item with
        {
            AuthorId = other.User.Id, PlaceId = other.Place.Id, ShoppingListItems = []
        };
        Seed(graph.Entities.Concat(other.Entities).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(updated);

        // Assert
        var saved = context.Items.Single(item => item.Id == graph.Item.Id);
        Assert.Equal(graph.Item.Id, updatedId);
        Assert.Equal(other.User.Id, saved.AuthorId);
        Assert.Equal(other.Place.Id, saved.PlaceId);
        Assert.Equal(2, context.Users.Count());
        Assert.Equal(2, context.Places.Count());
        Assert.Equal(graph.Place.Name, context.Places.Single(place => place.Id == graph.Place.Id).Name);
        Assert.Equal(other.Place.Name, context.Places.Single(place => place.Id == other.Place.Id).Name);
        Assert.Equal(graph.Item.Id, context.ShoppingListItems.Single(entry => entry.Id == graph.Entry.Id).ItemId);
    }

    [Fact]
    public void Entry_Update_Can_Move_To_Another_Item_And_List()
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        var updated = graph.Entry with { ItemId = other.Item.Id, ShoppingListId = other.ShoppingList.Id };
        Seed(graph.Entities.Concat(other.Entities).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShoppingListItemRepository>();
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(updated);

        // Assert
        var saved = context.ShoppingListItems.Single(entry => entry.Id == graph.Entry.Id);
        Assert.Equal(graph.Entry.Id, updatedId);
        Assert.Equal(other.Item.Id, saved.ItemId);
        Assert.Equal(other.ShoppingList.Id, saved.ShoppingListId);
        Assert.Equal(2, context.Items.Count());
        Assert.Equal(2, context.ShoppingLists.Count());
        Assert.Equal(2, context.ShoppingListItems.Count());
    }

    [Fact]
    public void ShoppingList_Update_Changes_Only_Scalars_And_Preserves_Author_And_Entries()
    {
        // Arrange
        var graph = new TestDataGraph();
        var otherUser = new TestDataGraph().User;
        var updated = graph.ShoppingList with
        {
            Name = "Updated", Shop = "New shop", Description = null, AuthorId = otherUser.Id, Items = []
        };
        Seed(graph.Entities.Append(otherUser).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShoppingListRepository>();
        _ = repository.GetById(graph.ShoppingList.Id);
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(updated);

        // Assert
        var saved = Assert.Single(context.ShoppingLists.ToList());
        Assert.Equal(graph.ShoppingList.Id, updatedId);
        Assert.Equal(updated.Name, saved.Name);
        Assert.Equal(updated.Shop, saved.Shop);
        Assert.Null(saved.Description);
        Assert.Equal(graph.User.Id, saved.AuthorId);
        Assert.Equal(graph.Entry.Id, Assert.Single(context.ShoppingListItems.ToList()).Id);
        Assert.Equal(graph.Item.Id, Assert.Single(context.Items.ToList()).Id);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingListItem")]
    public void Update_Rejects_A_Second_Instance_Of_An_Already_Tracked_Entity(string entityType)
    {
        // Arrange
        var graph = new TestDataGraph();
        var user = graph.User with { Name = "Updated", Items = [], Places = [], ShoppingLists = [] };
        var place = graph.Place with { Name = "Updated", Items = [] };
        var item = graph.Item with { Name = "Updated", ShoppingListItems = [] };
        var entry = graph.Entry with { Amount = 99 };
        Seed(graph.Entities);
        using var scope = CreateScope();
        var services = scope.ServiceProvider;
        Action update;
        switch (entityType)
        {
            case "User":
                var users = services.GetRequiredService<IUserRepository>();
                _ = users.GetById(graph.User.Id);
                update = () => users.Update(user);
                break;
            case "Place":
                var places = services.GetRequiredService<IPlaceRepository>();
                _ = places.GetById(graph.Place.Id);
                update = () => places.Update(place);
                break;
            case "Item":
                var items = services.GetRequiredService<IItemRepository>();
                _ = items.GetById(graph.Item.Id);
                update = () => items.Update(item);
                break;
            case "ShoppingListItem":
                var entries = services.GetRequiredService<IShoppingListItemRepository>();
                _ = entries.GetById(graph.Entry.Id);
                update = () => entries.Update(entry);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(entityType));
        }
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(update);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(graph.User.Name, Assert.Single(context.Users.ToList()).Name);
        Assert.Equal(graph.Place.Name, Assert.Single(context.Places.ToList()).Name);
        Assert.Equal(graph.Item.Name, Assert.Single(context.Items.ToList()).Name);
        Assert.Equal(graph.Entry.Amount, Assert.Single(context.ShoppingListItems.ToList()).Amount);
    }
}
