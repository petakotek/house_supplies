using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

[Trait("Category", "Integration")]
public sealed class MigrationTests(DbContextFixture fixture) : DalTestBase(fixture)
{
    [Fact]
    public void Migrate_Creates_All_Entity_Tables_From_An_Empty_Database()
    {
        // Arrange
        using var context = CreateDbContext();
        context.Database.EnsureDeleted();
        using var verification = CreateDbContext();

        // Act
        context.Database.Migrate();

        // Assert
        Assert.Empty(verification.Users.ToList());
        Assert.Empty(verification.Places.ToList());
        Assert.Empty(verification.Items.ToList());
        Assert.Empty(verification.ShoppingLists.ToList());
        Assert.Empty(verification.ShoppingListItems.ToList());
        Assert.Equal(verification.Database.GetMigrations().ToArray(),
            verification.Database.GetAppliedMigrations().ToArray());
        Assert.NotEmpty(verification.Database.GetAppliedMigrations());
        Assert.Empty(verification.Database.GetPendingMigrations());
    }

    [Fact]
    public void Migrated_Database_Persists_And_Loads_A_Complete_Related_Dataset()
    {
        // Arrange
        using (var context = CreateDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }
        var graph = new TestDataGraph();
        using var insertContext = CreateDbContext();
        insertContext.AddRange(graph.Entities);
        using var scope = CreateScope();
        var services = scope.ServiceProvider;
        var listRepository = services.GetRequiredService<IShoppingListRepository>();
        var itemRepository = services.GetRequiredService<IItemRepository>();

        // Act
        insertContext.SaveChanges();

        // Assert
        var list = listRepository.GetById(graph.ShoppingList.Id);
        Assert.NotNull(list);
        Assert.Equal(graph.ShoppingList.Name, list.Name);
        Assert.Equal(graph.ShoppingList.Shop, list.Shop);
        Assert.NotNull(list.Author);
        Assert.Equal(graph.User.Id, list.Author.Id);
        var entry = Assert.Single(list.Items);
        Assert.Equal(graph.Entry.Id, entry.Id);
        Assert.Equal(graph.Entry.Amount, entry.Amount);
        Assert.NotNull(entry.Item);
        Assert.Equal(graph.Item.Id, entry.Item.Id);
        Assert.Equal(graph.Item.ExpirationDate, entry.Item.ExpirationDate);
        Assert.Equal(graph.Item.Category, entry.Item.Category);
        var item = itemRepository.GetById(graph.Item.Id);
        Assert.NotNull(item);
        Assert.NotNull(item.Place);
        Assert.Equal(graph.Place.Id, item.Place.Id);
    }

    [Fact]
    public void Migrate_On_UpToDate_Database_Preserves_Data()
    {
        // Arrange
        using var context = CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        var graph = new TestDataGraph();
        Seed(graph.Entities);
        var appliedBefore = context.Database.GetAppliedMigrations().ToArray();
        using var verification = CreateDbContext();

        // Act
        context.Database.Migrate();

        // Assert
        Assert.Equal(appliedBefore, verification.Database.GetAppliedMigrations().ToArray());
        Assert.Equal(graph.User.Id, Assert.Single(verification.Users.ToList()).Id);
        Assert.Equal(graph.Place.Id, Assert.Single(verification.Places.ToList()).Id);
        Assert.Equal(graph.Item.Id, Assert.Single(verification.Items.ToList()).Id);
        Assert.Equal(graph.ShoppingList.Id, Assert.Single(verification.ShoppingLists.ToList()).Id);
        Assert.Equal(graph.Entry.Id, Assert.Single(verification.ShoppingListItems.ToList()).Id);
    }

    [Fact]
    public void Current_Model_Has_No_Changes_Missing_From_Migrations()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        Assert.False(hasPendingChanges);
    }
}
