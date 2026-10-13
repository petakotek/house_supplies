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

`SqliteDbContextFixture` creates an empty schema and a clean SQLite backup once
per test class. Derive your test class from `DalTestBase` and pass the fixture
through its constructor. xUnit supplies the fixture through the inherited
`IClassFixture<SqliteDbContextFixture>` interface.

Use `CreateScope()` to resolve repository interfaces and the context through
`scope.ServiceProvider.GetRequiredService<T>()`. Within a scope, repositories
share the same context. `CreateDbContext()` is also available as a shortcut that
resolves a context in a new scope. After every test, including a failed assertion,
the base class disposes all scopes and their services and restores
the database from the backup. You do not need to remove inserted rows or reset
updated values yourself. Fixture disposal removes the database and backup when
the class finishes. Do not override or hide the base class's cleanup method.

Tests within a class run sequentially. Different classes have separate databases
and backups, so they can run in parallel. The schema comes from `EnsureCreated`;
this setup does not test migrations.

Example:

```csharp
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.DAL.Tests;

public sealed class UserRepositoryTests : DalTestBase
{
    public UserRepositoryTests(SqliteDbContextFixture fixture) : base(fixture)
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
