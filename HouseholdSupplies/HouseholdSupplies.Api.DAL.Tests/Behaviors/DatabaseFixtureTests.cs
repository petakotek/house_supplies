using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Api.DAL.EF;
using HouseholdSupplies.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Behaviors;

public sealed class DatabaseFixtureTests : DalTestBase
{
    private readonly DbContextFixture fixture;

    public DatabaseFixtureTests(DbContextFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DependencyInjection_Resolves_All_Repositories_And_Scoped_Context()
    {
        // Arrange
        using var scope = CreateScope();
        using var otherScope = CreateScope();
        var services = scope.ServiceProvider;

        // Act
        var users = services.GetRequiredService<IUserRepository>();
        var places = services.GetRequiredService<IPlaceRepository>();
        var items = services.GetRequiredService<IItemRepository>();
        var lists = services.GetRequiredService<IShoppingListRepository>();
        var entries = services.GetRequiredService<IShoppingListItemRepository>();
        var context = services.GetRequiredService<HouseholdSuppliesDbContext>();
        var sameContext = services.GetRequiredService<HouseholdSuppliesDbContext>();
        var otherContext = otherScope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();

        // Assert
        Assert.NotNull(users);
        Assert.NotNull(places);
        Assert.NotNull(items);
        Assert.NotNull(lists);
        Assert.NotNull(entries);
        Assert.Same(context, sameContext);
        Assert.NotSame(context, otherContext);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestDatabase_Starts_With_All_Entity_Tables_Empty()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var isSqlite = context.Database.IsSqlite();
        var counts = new[] { context.Users.Count(), context.Places.Count(), context.Items.Count(),
            context.ShoppingLists.Count(), context.ShoppingListItems.Count() };

        // Assert
        Assert.True(isSqlite);
        Assert.All(counts, count => Assert.Equal(0, count));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestDatabase_Enforces_Ef_Project_Foreign_Key_Configuration()
    {
        // Arrange
        using var context = CreateDbContext();
        context.Places.Add(new PlaceEntity
        {
            Id = Guid.NewGuid(),
            Name = "Pantry",
            AuthorId = Guid.NewGuid()
        });

        // Act
        var exception = Record.Exception(() => context.SaveChanges());

        // Assert
        Assert.IsType<DbUpdateException>(exception);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestCleanup_Restores_Database_From_Backup()
    {
        // Arrange
        using var scope = new TestScope(fixture);
        var context = scope.OpenContext();
        context.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test user",
            ImageUrl = "user.png",
            Role = Role.User
        });
        context.SaveChanges();
        Assert.Single(context.Users.ToList());
        using var verificationContext = CreateDbContext();

        // Act
        scope.Dispose();

        // Assert
        AssertDatabaseEmpty(verificationContext);
    }

    private static void AssertDatabaseEmpty(HouseholdSuppliesDbContext context)
    {
        Assert.Empty(context.Users.ToList());
        Assert.Empty(context.Places.ToList());
        Assert.Empty(context.Items.ToList());
        Assert.Empty(context.ShoppingLists.ToList());
        Assert.Empty(context.ShoppingListItems.ToList());
    }

    private sealed class TestScope(DbContextFixture fixture) : DalTestBase(fixture)
    {
        public HouseholdSuppliesDbContext OpenContext() => CreateDbContext();
    }
}
