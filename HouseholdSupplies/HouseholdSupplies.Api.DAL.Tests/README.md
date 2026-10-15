# Writing DAL integration tests

The test database is a real SQLite database stored in a unique temporary file.
It uses the same EF Core provider as the DAL, without accessing your application
database or user secrets.

The fixture calls the EF project's `ApiDALEFInstaller` to register the context,
repositories, and mappers in a dependency injection container. It resolves
`HouseholdSuppliesDbContext` from that container. Its
`OnModelCreating` applies all configurations from that project's assembly, so
tables, required fields, relationships, and delete rules use the same model as
the application. There is no separate test model. All five entity tables start
empty, and the backup restores that empty state after each test. The only
connection differences are the temporary file path, explicitly enabled foreign
keys, and disabled pooling so connections close before restoration.

`DbContextFixture` creates an empty schema and a clean SQLite backup once
per test class. Derive your test class from `DalTestBase` and pass the fixture
through its constructor. xUnit supplies the fixture through the inherited
`IClassFixture<DbContextFixture>` interface.

Use `CreateScope()` to resolve repository interfaces and the context through
`scope.ServiceProvider.GetRequiredService<T>()`. Within a scope, repositories
share the same context. `CreateDbContext()` is also available as a shortcut that
resolves a context in a new scope. After every test, including a failed assertion,
the base class disposes all scopes and their services and restores
the database from the backup. You do not need to remove inserted rows or reset
updated values yourself. Fixture disposal removes the database and backup when
the class finishes. Do not override or hide the base class's cleanup method.

Tests within a class run sequentially. Different classes have separate databases
and backups, so they can run in parallel. Regular tests use `EnsureCreated`.
`MigrationTests` removes that schema and uses the EF project's actual migrations
in the isolated test database. The normal empty backup is still restored afterward.

## Coverage in implementation order

1. Database fixture and dependency injection: empty tables, repository resolution,
   context lifetimes, and backup restoration.
2. UserRepository: CRUD, missing records, detached and tracked updates.
3. PlaceRepository: CRUD and persisted scalar/foreign-key values.
4. ItemRepository: CRUD, amounts, categories, expiration dates, and optional fields.
5. ShoppingListRepository: CRUD and scalar updates.
6. ShoppingListItemRepository: CRUD, amounts, bought status, and parent IDs.
7. Relationship loading: authors, places, empty/populated collections, and nested items.
8. Missing records: empty GUID reads, updates, and deletes in populated databases.
9. Constraints: all foreign keys, required strings, duplicate IDs, and nullable values.
10. Delete restrictions and cascades: preserve parents and unrelated records; check
    both repository deletion and database deletion without tracking dependents.
11. Update edge cases: preserve related records, move foreign keys, scalar-only
    shopping-list updates, and reject conflicting tracked instances.
12. Migrations: empty schema creation, related data, repeated migration application,
    and missing model changes.

Every test has separate Arrange, Act, and Assert sections. `Seed(...)` persists
test data through a separate DI scope. Assertions match records by ID rather than
depending on database result order. Expected failures are captured in Act with
`Record.Exception` and checked in Assert.

The generic repository accepts detached updates in a fresh scope and updates to
the instance already tracked in its scope. Attaching a second instance with the
same ID while the original is tracked throws; this current behavior is covered.
The shopping-list override updates only Name, Shop, and Description, preserving
its author and entries. All repositories return null for a missing update.

Example:

```csharp
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.DAL.Tests;

public sealed class UserRepositoryTests : DalTestBase
{
    public UserRepositoryTests(DbContextFixture fixture) : base(fixture)
    {
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Insert_Saves_User_To_Database()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test user",
            ImageUrl = "user.png",
            Role = Role.User
        };

        // Act
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            repository.Insert(user);
        }

        // Assert: use a fresh context to verify persistence.
        using var verificationContext = CreateDbContext();
        var savedUser = Assert.Single(verificationContext.Users.ToList());
        Assert.Equal(user.Id, savedUser.Id);
        Assert.Equal(user.Name, savedUser.Name);
    }
}
```

Run from the repository root with .NET 10:

```sh
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release
```

Only integration tests:

```sh
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release --filter "Category=Integration"
```
