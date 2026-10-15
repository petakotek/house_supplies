using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Repositories;

[Trait("Category", "Integration")]
public sealed class ShoppingListRepositoryTests : DalTestBase
{
    private readonly IShoppingListRepository repository;

    public ShoppingListRepositoryTests(DbContextFixture fixture) : base(fixture)
    {
        var scope = CreateScope();
        repository = scope.ServiceProvider.GetRequiredService<IShoppingListRepository>();
    }

    [Fact]
    public void Insert_Saves_ShoppingList_To_Database()
    {
        // Arrange
        var entity = CreateEntity();
        using var context = CreateDbContext();

        // Act
        var insertedId = repository.Insert(entity);

        // Assert
        var saved = Assert.Single(context.ShoppingLists.ToList());
        Assert.Equal(entity.Id, insertedId);
        AssertFields(entity, saved);
    }

    [Fact]
    public void GetById_Returns_Existing_ShoppingList()
    {
        // Arrange
        var entity = CreateEntity();
        Seed(entity);

        // Act
        var retrieved = repository.GetById(entity.Id);

        // Assert
        Assert.NotNull(retrieved);
        AssertFields(entity, retrieved);
    }

    [Fact]
    public void GetAll_Returns_All_ShoppingLists()
    {
        // Arrange
        var first = CreateEntity();
        var second = CreateEntity();
        Seed(first, second);

        // Act
        var retrieved = repository.GetAll();

        // Assert
        Assert.Equal(2, retrieved.Count);
        AssertFields(first, Assert.Single(retrieved, entity => entity.Id == first.Id));
        AssertFields(second, Assert.Single(retrieved, entity => entity.Id == second.Id));
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection()
    {
        // Arrange
        var scopedRepository = repository;

        // Act
        var retrieved = scopedRepository.GetAll();

        // Assert
        Assert.Empty(retrieved);
    }

    [Fact]
    public void Exists_Returns_True_For_Existing_Entity()
    {
        // Arrange
        var entity = CreateEntity();
        Seed(entity);

        // Act
        var exists = repository.Exists(entity.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void Exists_Returns_False_For_Missing_Entity()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var exists = repository.Exists(missingId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void Update_Saves_Detached_Entity_And_Preserves_Other_Records()
    {
        // Arrange
        var entity = CreateEntity();
        var other = CreateEntity();
        Seed(entity, other);
        var updated = entity with { Name = "Updated list", Shop = "New shop", Description = null };
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(updated);

        // Assert
        Assert.Equal(entity.Id, updatedId);
        Assert.Equal(2, context.ShoppingLists.Count());
        AssertFields(updated, context.ShoppingLists.Single(value => value.Id == entity.Id));
        AssertFields(other, context.ShoppingLists.Single(value => value.Id == other.Id));
    }

    [Fact]
    public void Update_Saves_Tracked_Entity()
    {
        // Arrange
        var entity = CreateEntity();
        Seed(entity);
        var tracked = repository.GetById(entity.Id)
            ?? throw new InvalidOperationException("Seeded entity was not found.");
        tracked.Name = "Updated list";
        tracked.Shop = "New shop";
        tracked.Description = null;
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(tracked);

        // Assert
        Assert.Equal(entity.Id, updatedId);
        AssertFields(tracked, Assert.Single(context.ShoppingLists.ToList()));
    }

    [Fact]
    public void Remove_Deletes_Only_Selected_Entity()
    {
        // Arrange
        var entity = CreateEntity();
        var other = CreateEntity();
        Seed(entity, other);
        using var context = CreateDbContext();

        // Act
        repository.Remove(entity.Id);

        // Assert
        AssertFields(other, Assert.Single(context.ShoppingLists.ToList()));
    }

    [Fact]
    public void GetById_Returns_Null_For_Missing_Entity()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var retrieved = repository.GetById(missingId);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_Entity()
    {
        // Arrange
        var entity = CreateEntity();
        using var context = CreateDbContext();

        // Act
        var updatedId = repository.Update(entity);

        // Assert
        Assert.Null(updatedId);
        Assert.Empty(context.ShoppingLists.ToList());
    }

    [Fact]
    public void Remove_Does_Nothing_For_Missing_Entity()
    {
        // Arrange
        var entity = CreateEntity();
        Seed(entity);
        var missingId = Guid.NewGuid();
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(() => repository.Remove(missingId));

        // Assert
        Assert.Null(exception);
        AssertFields(entity, Assert.Single(context.ShoppingLists.ToList()));
    }

    private ShoppingListEntity CreateEntity()
    {
        var graph = new TestDataGraph();
        Seed(graph.User);
        return graph.ShoppingList;
    }

    private static void AssertFields(ShoppingListEntity expected, ShoppingListEntity actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Shop, actual.Shop);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.AuthorId, actual.AuthorId);
    }
}
