# Writing DAL integration tests

All DAL and API integration tests use the production Azure SQL EF provider and
production migrations. There is one shared schema and migration history to maintain.
No application database or developer user secrets are used by tests.

## Configure a test server

Set `HouseholdSuppliesTests__ConnectionString` to a **dedicated test server** connection.
The login needs permission to create and drop databases. This can be an Azure SQL
logical test server, SQL Server Developer Edition, or Windows SQL Server LocalDB.
Tests never fall back to the application's connection string.

PowerShell (Windows LocalDB):

```powershell
sqllocaldb start MSSQLLocalDB
$env:HouseholdSuppliesTests__ConnectionString = 'Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True'
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release
```

For Azure SQL, supply your test server connection through the same environment
variable using your secret manager or shell. Keep credentials out of source control.
Allow the test runner through the test server firewall. On macOS/Linux, use an
accessible SQL Server or Azure SQL test server. The pipeline starts LocalDB on its
Windows agent and sets this variable for the test step.

## Isolation and lifecycle

The shared `SqlServerTestDatabase` helper in `HouseholdSupplies.Api.DAL/TestInfrastructure`
is referenced by both test projects through the DAL assembly. It replaces the supplied catalog with a
unique `HouseholdSuppliesTests_<guid>` database, applies `Database.Migrate()`, and
drops only that database after the fixture finishes. Azure SQL runs can therefore
create several temporary databases and incur charges. A failed migration triggers
best-effort database cleanup; if a test process is forcibly terminated, remove its
leftover test databases manually.

`DbContextFixture` registers the real `ApiDALInstaller`, repositories, and mappers
with the isolated connection. Derive DAL test classes from `DalTestBase` and accept
`DbContextFixture` in the constructor; xUnit provides one fixture per class.
`CreateScope()` and `CreateDbContext()` resolve production services. After each test,
including a failed assertion, the base class disposes its scopes and the fixture
restores the current migration version and deletes rows in foreign-key order.
Do not override or hide the base class cleanup method. Tests in a class run
sequentially; different classes use different databases and can run in parallel.

API endpoint tests use the same helper through an xUnit class fixture, dispose the
HTTP client/application host, and clear their database between tests. Migration
tests exercise the production migration chain, including fresh creation, upgrades
preserving existing data, rollback, repeated application, and model consistency.
Offline SQL script and design-time configuration tests do not require a server.

## Coverage in implementation order

1. Database fixture and dependency injection: empty tables, repository resolution,
   context lifetimes, and database reset.
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
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
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

Run only tests that do not require a database server:

```sh
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release --filter "Category!=Integration"
```

Integration tests fail with a configuration message if the dedicated connection
variable is missing; they are not silently skipped.
