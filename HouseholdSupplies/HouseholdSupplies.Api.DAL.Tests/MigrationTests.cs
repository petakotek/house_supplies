using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HouseholdSupplies.Api.DAL.Tests;

[Trait("Category", "Integration")]
public sealed class MigrationTests : DalTestBase
{
    public MigrationTests(DbContextFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void SqlServer_Migrations_Create_Schema_And_Preserve_Related_Data_When_Reapplied()
    {
        // Arrange: reset only this fixture's isolated database to before the first migration.
        using var context = CreateDbContext();
        context.GetService<IMigrator>().Migrate(Migration.InitialDatabase);
        var graph = new TestDataGraph();
        graph.User.OwnerId = "test-owner";

        // Act
        context.Database.Migrate();
        context.AddRange(graph.Entities);
        context.SaveChanges();
        var firstAppliedMigrations = context.Database.GetAppliedMigrations().ToArray();
        context.Database.Migrate();

        // Assert: all migrations ran, the second application was a no-op, and relations survived.
        Assert.NotEmpty(firstAppliedMigrations);
        Assert.Equal(context.Database.GetMigrations(), firstAppliedMigrations);
        Assert.Equal(firstAppliedMigrations, context.Database.GetAppliedMigrations());
        using var verificationContext = CreateDbContext();
        var entry = Assert.Single(verificationContext.ShoppingListItems
            .Include(item => item.ShoppingList)
            .Include(item => item.Item)
            .ToList());
        Assert.NotNull(entry.Item);
        Assert.NotNull(entry.ShoppingList);
        var place = Assert.Single(verificationContext.Places.ToList());
        var user = Assert.Single(verificationContext.Users.ToList());
        Assert.Equal(graph.Entry.Id, entry.Id);
        Assert.Equal(graph.Item.Id, entry.Item.Id);
        Assert.Equal(graph.Place.Id, place.Id);
        Assert.Equal(place.Id, entry.Item.PlaceId);
        Assert.Equal(graph.ShoppingList.Id, entry.ShoppingList.Id);
        Assert.Equal(graph.User.Id, user.Id);
        Assert.Equal(user.Id, entry.Item.AuthorId);
        Assert.Equal("test-owner", user.OwnerId);
        Assert.Equal(graph.Item.Amount, entry.Item.Amount);
        Assert.Equal(graph.Item.ExpirationDate, entry.Item.ExpirationDate);
    }

    [Fact]
    public void OwnerId_Migration_Preserves_Existing_Rows()
    {
        // Arrange
        using var context = CreateDbContext();
        context.GetService<IMigrator>().Migrate(Migration.InitialDatabase);
        var migrator = context.GetService<IMigrator>();
        migrator.Migrate("20261011163919_InitialMigration");
        // The initial schema predates OwnerId, so seed through its actual SQL columns.
        var id = Guid.NewGuid();
        context.Database.ExecuteSqlInterpolated(
            $"INSERT INTO Users (Id, Name, ImageUrl, Role) VALUES ({id}, {"Existing user"}, {"user.png"}, {0})");

        // Act
        context.Database.Migrate();

        // Assert
        var user = Assert.Single(context.Users.ToList());
        Assert.Equal(id, user.Id);
        Assert.Equal("Existing user", user.Name);
        Assert.Null(user.OwnerId);
    }

    [Fact]
    public void SqlServer_Model_Has_No_Pending_Migration_Changes()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        Assert.False(hasPendingChanges);
    }
}
