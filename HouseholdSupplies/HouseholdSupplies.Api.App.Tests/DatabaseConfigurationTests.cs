using HouseholdSupplies.Api.DAL.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace HouseholdSupplies.Api.App.Tests;

public sealed class DatabaseConfigurationTests
{
    [Theory]
    [InlineData("Development", "")]
    [InlineData("Development", "   ")]
    [InlineData("Production", "")]
    [InlineData("Production", "   ")]
    public void EmptyConnectionString_PreventsStartup_WithConfigurationMessage(string environment, string connectionString)
    {
        // Arrange: explicitly override any developer secrets or environment settings.
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment(environment)
                .UseSetting("ConnectionStrings:DefaultConnection", connectionString));

        // Act
        var exception = Record.Exception(() => factory.CreateClient());

        // Assert
        var configurationError = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("ConnectionStrings:DefaultConnection", configurationError.Message);
        Assert.Contains("ConnectionStrings__DefaultConnection", configurationError.Message);
    }

    [Fact]
    public void ApiHost_UsesSameDeveloperSecrets_AsMigrationFactory()
    {
        // Arrange
        var apiAssembly = typeof(Program).Assembly;
        var dalAssembly = typeof(HouseholdSuppliesDbContextFactory).Assembly;

        // Act
        var apiSecrets = Attribute.GetCustomAttribute(apiAssembly, typeof(UserSecretsIdAttribute)) as UserSecretsIdAttribute;
        var dalSecrets = Attribute.GetCustomAttribute(dalAssembly, typeof(UserSecretsIdAttribute)) as UserSecretsIdAttribute;

        // Assert
        Assert.NotNull(apiSecrets);
        Assert.NotNull(dalSecrets);
        Assert.Equal(dalSecrets.UserSecretsId, apiSecrets.UserSecretsId);
    }
}
