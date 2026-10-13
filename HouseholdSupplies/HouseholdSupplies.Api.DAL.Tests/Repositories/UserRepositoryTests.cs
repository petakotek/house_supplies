using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests.Repositories;

[Trait("Category", "Integration")]
public sealed class UserRepositoryTests : DalTestBase
{
    private readonly IUserRepository userRepository;

    public UserRepositoryTests(DbContextFixture fixture) : base(fixture)
    {
        var scope = CreateScope();
        userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Insert_Saves_User_To_Database()
    {
        // Arrange
        var user = CreateUser();
        using var testContext = CreateDbContext();

        // Act
        var insertedId = userRepository.Insert(user);

        // Assert
        var savedUser = Assert.Single(testContext.Users.ToList());

        Assert.Equal(user.Id, insertedId);
        Assert.Equal(user.Id, savedUser.Id);
        Assert.Equal(user.Name, savedUser.Name);
        Assert.Equal(user.ImageUrl, savedUser.ImageUrl);
        Assert.Equal(user.Role, savedUser.Role);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetById_Returns_Existing_User()
    {
        // Arrange
        var user = CreateUser();
        using (var testContext = CreateDbContext())
        {
            testContext.Users.Add(user);
            testContext.SaveChanges();
        }

        // Act
        var retrievedUser = userRepository.GetById(user.Id);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Name, retrievedUser.Name);
        Assert.Equal(user.ImageUrl, retrievedUser.ImageUrl);
        Assert.Equal(user.Role, retrievedUser.Role);
    }

    [Fact]
    public void GetById_Returns_Null_For_Missing_User()
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        var retrievedUser = userRepository.GetById(testId);

        // Assert
        Assert.Null(retrievedUser);
    }

    [Fact]
    public void GetAll_Returns_All_Users()
    {
        // Arrange
        var users = new[]
        {
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test user",
                ImageUrl = "Test imageUrl",
                Role = Role.User
            },
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test admin",
                ImageUrl = "Test imageUrl",
                Role = Role.Admin
            }
        };

        using (var testContext = CreateDbContext())
        {
            testContext.Users.AddRange(users);
            testContext.SaveChanges();
        }

        // Act
        var retrievedUsers = userRepository.GetAll();

        // Assert
        Assert.Equal(users.Length, retrievedUsers.Count);
        foreach (var expectedUser in users)
        {
            var retrievedUser = Assert.Single(retrievedUsers,
                user => user.Id == expectedUser.Id);

            Assert.Equal(expectedUser.Name, retrievedUser.Name);
            Assert.Equal(expectedUser.ImageUrl, retrievedUser.ImageUrl);
            Assert.Equal(expectedUser.Role, retrievedUser.Role);
        }
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_When_Database_Is_Empty()
    {
        // Arrange
        var repository = userRepository;

        // Act
        var users = repository.GetAll();

        // Assert
        Assert.Empty(users);
    }

    [Fact]
    public void Exists_Returns_True_For_Existing_User()
    {
        // Arrange
        var user = CreateUser();
        Seed(user);

        // Act
        var exists = userRepository.Exists(user.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void Exists_Returns_False_For_Missing_User()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var exists = userRepository.Exists(missingId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void Update_Saves_Changes_For_Existing_User()
    {
        // Arrange
        var user = CreateUser();
        var otherUser = CreateUser();
        Seed(user, otherUser);
        var updatedUser = user with { Name = "Updated user", ImageUrl = "updated.png", Role = Role.Admin };
        using var context = CreateDbContext();

        // Act
        var updatedId = userRepository.Update(updatedUser);

        // Assert
        var savedUser = context.Users.Single(entity => entity.Id == user.Id);
        Assert.Equal(user.Id, updatedId);
        Assert.Equal(updatedUser.Name, savedUser.Name);
        Assert.Equal(updatedUser.ImageUrl, savedUser.ImageUrl);
        Assert.Equal(updatedUser.Role, savedUser.Role);
        var unchangedUser = context.Users.Single(entity => entity.Id == otherUser.Id);
        Assert.Equal(otherUser.Name, unchangedUser.Name);
        Assert.Equal(2, context.Users.Count());
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_User()
    {
        // Arrange
        var user = CreateUser();
        using var context = CreateDbContext();

        // Act
        var updatedId = userRepository.Update(user);

        // Assert
        Assert.Null(updatedId);
        Assert.Empty(context.Users.ToList());
    }

    [Fact]
    public void Update_Saves_Changes_For_Tracked_User()
    {
        // Arrange
        var user = CreateUser();
        Seed(user);
        var trackedUser = userRepository.GetById(user.Id)
            ?? throw new InvalidOperationException("Seeded user was not found.");
        trackedUser.Name = "Updated tracked user";
        trackedUser.ImageUrl = "tracked.png";
        trackedUser.Role = Role.Admin;
        using var context = CreateDbContext();

        // Act
        var updatedId = userRepository.Update(trackedUser);

        // Assert
        var savedUser = Assert.Single(context.Users.ToList());
        Assert.Equal(user.Id, updatedId);
        Assert.Equal(trackedUser.Name, savedUser.Name);
        Assert.Equal(trackedUser.ImageUrl, savedUser.ImageUrl);
        Assert.Equal(trackedUser.Role, savedUser.Role);
    }

    [Fact]
    public void Remove_Deletes_Existing_User()
    {
        // Arrange
        var user = CreateUser();
        var otherUser = CreateUser();
        Seed(user, otherUser);
        using var context = CreateDbContext();

        // Act
        userRepository.Remove(user.Id);

        // Assert
        var remainingUser = Assert.Single(context.Users.ToList());
        Assert.Equal(otherUser.Id, remainingUser.Id);
    }

    [Fact]
    public void Remove_Does_Nothing_For_Missing_User()
    {
        // Arrange
        var user = CreateUser();
        Seed(user);
        var missingId = Guid.NewGuid();
        using var context = CreateDbContext();

        // Act
        var exception = Record.Exception(() => userRepository.Remove(missingId));

        // Assert
        Assert.Null(exception);
        Assert.Equal(user.Id, Assert.Single(context.Users.ToList()).Id);
    }
    private static UserEntity CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test user",
        ImageUrl = "user.png",
        Role = Role.User
    };
}
