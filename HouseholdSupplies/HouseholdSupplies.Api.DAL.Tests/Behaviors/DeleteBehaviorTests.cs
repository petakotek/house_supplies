using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Api.DAL.EF;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class DeleteBehaviorTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Theory]
    [InlineData("Place")]
    [InlineData("Item")]
    [InlineData("ShoppingList")]
    public void Remove_User_Is_Blocked_When_Referenced(string reference)
    {
        // Arrange
        var graph = new TestDataGraph();
        if (reference == "Item")
        {
            var otherUser = new TestDataGraph().User;
            graph.Place.AuthorId = otherUser.Id;
            Seed(graph.User, otherUser, graph.Place, graph.Item);
        }
        else if (reference == "Place")
        {
            Seed(graph.User, graph.Place);
        }
        else
        {
            Seed(graph.User, graph.ShoppingList);
        }
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(() => repository.Remove(graph.User.Id));

        // Assert
        var updateException = Assert.IsType<DbUpdateException>(exception);
        var sqliteException = Assert.IsType<SqliteException>(updateException.InnerException);
        Assert.Equal(19, sqliteException.SqliteErrorCode);
        Assert.Equal(1811, sqliteException.SqliteExtendedErrorCode);
        Assert.Contains("FOREIGN KEY constraint failed", sqliteException.Message);
        Assert.True(context.Users.Any(user => user.Id == graph.User.Id));
        if (reference == "Item")
            Assert.Equal(graph.Item.Id, Assert.Single(context.Items.ToList()).Id);
        else if (reference == "Place")
            Assert.Equal(graph.Place.Id, Assert.Single(context.Places.ToList()).Id);
        else
            Assert.Equal(graph.ShoppingList.Id, Assert.Single(context.ShoppingLists.ToList()).Id);
    }

    [Fact]
    public void Remove_Place_With_Items_Is_Blocked()
    {
        // Arrange
        var graph = new TestDataGraph();
        Seed(graph.User, graph.Place, graph.Item);
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPlaceRepository>();
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(() => repository.Remove(graph.Place.Id));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(graph.Place.Id, Assert.Single(context.Places.ToList()).Id);
        Assert.Equal(graph.Item.Id, Assert.Single(context.Items.ToList()).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Remove_ShoppingList_Deletes_Entries_And_Preserves_Items(bool deleteDirectly)
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        Seed(graph.Entities.Concat(other.Entities).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShoppingListRepository>();
        var context = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
        using var verification = CreateDbContext();

        // Act
        if (deleteDirectly)
            context.ShoppingLists.Where(list => list.Id == graph.ShoppingList.Id).ExecuteDelete();
        else
            repository.Remove(graph.ShoppingList.Id);

        // Assert
        Assert.Equal(other.ShoppingList.Id, Assert.Single(verification.ShoppingLists.ToList()).Id);
        Assert.Equal(other.Entry.Id, Assert.Single(verification.ShoppingListItems.ToList()).Id);
        Assert.Equal(2, verification.Items.Count());
        Assert.True(verification.Items.Any(item => item.Id == graph.Item.Id));
        Assert.Equal(2, verification.Users.Count());
        Assert.Equal(2, verification.Places.Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Remove_Item_Deletes_Entries_Across_Lists_And_Preserves_Lists(bool deleteDirectly)
    {
        // Arrange
        var graph = new TestDataGraph();
        var other = new TestDataGraph();
        var sharedEntry = graph.Entry with
        {
            Id = Guid.NewGuid(), ShoppingListId = other.ShoppingList.Id
        };
        Seed(graph.Entities.Concat(other.Entities).Append(sharedEntry).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
        var context = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
        using var verification = CreateDbContext();

        // Act
        if (deleteDirectly)
            context.Items.Where(item => item.Id == graph.Item.Id).ExecuteDelete();
        else
            repository.Remove(graph.Item.Id);

        // Assert
        Assert.Equal(other.Item.Id, Assert.Single(verification.Items.ToList()).Id);
        Assert.Equal(other.Entry.Id, Assert.Single(verification.ShoppingListItems.ToList()).Id);
        Assert.Equal(2, verification.ShoppingLists.Count());
        Assert.Equal(2, verification.Places.Count());
        Assert.Equal(2, verification.Users.Count());
    }

    [Fact]
    public void Remove_Entry_Preserves_Parents_And_Other_Entries()
    {
        // Arrange
        var graph = new TestDataGraph();
        var otherEntry = graph.Entry with { Id = Guid.NewGuid(), IsBought = true };
        Seed(graph.Entities.Append(otherEntry).ToArray());
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShoppingListItemRepository>();
        using var context = CreateDbContext();

        // Act
        repository.Remove(graph.Entry.Id);

        // Assert
        Assert.Equal(otherEntry.Id, Assert.Single(context.ShoppingListItems.ToList()).Id);
        Assert.Equal(graph.Item.Id, Assert.Single(context.Items.ToList()).Id);
        Assert.Equal(graph.ShoppingList.Id, Assert.Single(context.ShoppingLists.ToList()).Id);
        Assert.Equal(graph.User.Id, Assert.Single(context.Users.ToList()).Id);
        Assert.Equal(graph.Place.Id, Assert.Single(context.Places.ToList()).Id);
    }
}
