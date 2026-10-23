using HouseholdSupplies.Api.DAL.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace HouseholdSupplies.Api.DAL.Tests;

[CollectionDefinition("Process environment", DisableParallelization = true)]
public sealed class ProcessEnvironmentCollection;

[Collection("Process environment")]
public sealed class DesignTimeDbContextFactoryTests
{
    [Fact]
    public void Environment_Connection_String_Overrides_Developer_User_Secrets()
    {
        // Arrange: preserve the process setting without modifying developer secrets.
        const string variable = "ConnectionStrings__DefaultConnection";
        const string connectionString =
            "Server=localhost;Database=EnvironmentMigrationTests;Integrated Security=True;TrustServerCertificate=True";
        var previousValue = Environment.GetEnvironmentVariable(variable);

        try
        {
            Environment.SetEnvironmentVariable(variable, connectionString);
            var factory = new HouseholdSuppliesDbContextFactory();

            // Act: constructing a context does not connect to the server.
            using var context = factory.CreateDbContext([]);

            // Assert: pipeline configuration takes precedence over any local secret.
            Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
            var connection = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
            Assert.Equal("localhost", connection.DataSource);
            Assert.Equal("EnvironmentMigrationTests", connection.InitialCatalog);
            Assert.True(connection.IntegratedSecurity);
            Assert.True(connection.TrustServerCertificate);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, previousValue);
        }
    }
}
