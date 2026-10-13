using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class DatabaseConstraintTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Theory]
    [InlineData("Place.Author")]
    [InlineData("Item.Author")]
    [InlineData("Item.Place")]
    [InlineData("ShoppingList.Author")]
    [InlineData("ShoppingListItem.Item")]
    [InlineData("ShoppingListItem.ShoppingList")]
    public void SaveChanges_Rejects_Nonexistent_Foreign_Key(string relationship)
    {
        // Arrange
        var graph = new TestDataGraph();
        var missingId = Guid.NewGuid();
        switch (relationship)
        {
            case "Place.Author": graph.Place.AuthorId = missingId; break;
            case "Item.Author": graph.Item.AuthorId = missingId; break;
            case "Item.Place": graph.Item.PlaceId = missingId; break;
            case "ShoppingList.Author": graph.ShoppingList.AuthorId = missingId; break;
            case "ShoppingListItem.Item": graph.Entry.ItemId = missingId; break;
            case "ShoppingListItem.ShoppingList": graph.Entry.ShoppingListId = missingId; break;
            default: throw new ArgumentOutOfRangeException(nameof(relationship));
        }
        using var context = CreateDbContext();
        context.AddRange(graph.Entities);
        using var verification = CreateDbContext();

        // Act
        var exception = Record.Exception(() => context.SaveChanges());

        // Assert
        AssertSqliteConstraint(exception, 787);
        Assert.Empty(verification.Users.ToList());
        Assert.Empty(verification.Places.ToList());
        Assert.Empty(verification.Items.ToList());
        Assert.Empty(verification.ShoppingLists.ToList());
        Assert.Empty(verification.ShoppingListItems.ToList());
    }

    [Theory]
    [InlineData("User.Name")]
    [InlineData("User.ImageUrl")]
    [InlineData("Place.Name")]
    [InlineData("Item.Name")]
    [InlineData("Item.ImageUrl")]
    [InlineData("ShoppingList.Name")]
    [InlineData("ShoppingList.Shop")]
    public void SaveChanges_Rejects_Null_Required_String(string property)
    {
        // Arrange
        var graph = new TestDataGraph();
        switch (property)
        {
            case "User.Name": graph.User.Name = null!; break;
            case "User.ImageUrl": graph.User.ImageUrl = null!; break;
            case "Place.Name": graph.Place.Name = null!; break;
            case "Item.Name": graph.Item.Name = null!; break;
            case "Item.ImageUrl": graph.Item.ImageUrl = null!; break;
            case "ShoppingList.Name": graph.ShoppingList.Name = null!; break;
            case "ShoppingList.Shop": graph.ShoppingList.Shop = null!; break;
            default: throw new ArgumentOutOfRangeException(nameof(property));
        }
        using var context = CreateDbContext();
        context.AddRange(graph.Entities);

        // Act
        var exception = Record.Exception(() => context.SaveChanges());

        // Assert
        AssertSqliteConstraint(exception, 1299);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    [InlineData("ShoppingListItem")]
    public void Insert_Rejects_Duplicate_Id_Without_Changing_Existing_Record(string entityType)
    {
        // Arrange
        var graph = new TestDataGraph();
        var duplicateUser = graph.User with { Name = "Duplicate", Items = [], Places = [], ShoppingLists = [] };
        var duplicatePlace = graph.Place with { Name = "Duplicate", Items = [] };
        var duplicateItem = graph.Item with { Name = "Duplicate", ShoppingListItems = [] };
        var duplicateList = graph.ShoppingList with { Name = "Duplicate", Items = [] };
        var duplicateEntry = graph.Entry with { Amount = 99 };
        Seed(graph.Entities);
        using var scope = CreateScope();
        var services = scope.ServiceProvider;
        var resolvedUserRepository = services.GetRequiredService<IUserRepository>();
        var resolvedPlaceRepository = services.GetRequiredService<IPlaceRepository>();
        var resolvedItemRepository = services.GetRequiredService<IItemRepository>();
        var resolvedShoppingListRepository = services.GetRequiredService<IShoppingListRepository>();
        var resolvedShoppingListItemRepository = services.GetRequiredService<IShoppingListItemRepository>();
        Action insert = entityType switch
        {
            "User" => () => resolvedUserRepository.Insert(duplicateUser),
            "Place" => () => resolvedPlaceRepository.Insert(duplicatePlace),
            "Item" => () => resolvedItemRepository.Insert(duplicateItem),
            "ShoppingList" => () => resolvedShoppingListRepository.Insert(duplicateList),
            "ShoppingListItem" => () => resolvedShoppingListItemRepository.Insert(duplicateEntry),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(insert);

        // Assert
        AssertSqliteConstraint(exception, 1555);
        Assert.Equal(graph.User.Name, Assert.Single(context.Users.ToList()).Name);
        Assert.Equal(graph.Place.Name, Assert.Single(context.Places.ToList()).Name);
        Assert.Equal(graph.Item.Name, Assert.Single(context.Items.ToList()).Name);
        Assert.Equal(graph.ShoppingList.Name, Assert.Single(context.ShoppingLists.ToList()).Name);
        Assert.Equal(graph.Entry.Amount, Assert.Single(context.ShoppingListItems.ToList()).Amount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Optional_Fields_RoundTrip_As_Null_Or_Values(bool useNulls)
    {
        // Arrange
        var graph = new TestDataGraph();
        if (useNulls)
        {
            graph.Place.ImageUrl = null;
            graph.Place.Description = null;
            graph.Item.Description = null;
            graph.Item.ExpirationDate = null;
            graph.ShoppingList.Description = null;
        }
        using var context = CreateDbContext();
        context.AddRange(graph.Entities);
        using var verification = CreateDbContext();

        // Act
        context.SaveChanges();

        // Assert
        var place = Assert.Single(verification.Places.ToList());
        var item = Assert.Single(verification.Items.ToList());
        var list = Assert.Single(verification.ShoppingLists.ToList());
        Assert.Equal(graph.Place.ImageUrl, place.ImageUrl);
        Assert.Equal(graph.Place.Description, place.Description);
        Assert.Equal(graph.Item.Description, item.Description);
        Assert.Equal(graph.Item.ExpirationDate, item.ExpirationDate);
        Assert.Equal(graph.ShoppingList.Description, list.Description);
    }

    private static void AssertSqliteConstraint(Exception? exception, int extendedCode)
    {
        var updateException = Assert.IsType<DbUpdateException>(exception);
        var sqliteException = Assert.IsType<SqliteException>(updateException.InnerException);
        Assert.Equal(19, sqliteException.SqliteErrorCode);
        Assert.Equal(extendedCode, sqliteException.SqliteExtendedErrorCode);
    }
}
