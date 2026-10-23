using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.TestInfrastructure;

/// <summary>Owns a unique database on a dedicated SQL Server or Azure SQL test server.</summary>
public sealed class SqlServerTestDatabase : IDisposable
{
    public const string ConnectionVariable = "HouseholdSuppliesTests__ConnectionString";
    public string ConnectionString { get; }

    public SqlServerTestDatabase()
    {
        var configuredConnection = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            throw new InvalidOperationException(
                $"Set {ConnectionVariable} to a dedicated SQL Server/Azure SQL test server connection. " +
                "The test login must be able to create and drop isolated test databases. " +
                "Application connection strings and user secrets are never used by integration tests.");
        }

        var connection = new SqlConnectionStringBuilder(configuredConnection);
        if (!string.IsNullOrEmpty(connection.AttachDBFilename))
        {
            throw new InvalidOperationException(
                $"{ConnectionVariable} must specify a test server, without AttachDBFilename.");
        }

        // Always replace the catalog; never migrate, empty, or drop the supplied database.
        connection.InitialCatalog = $"HouseholdSuppliesTests_{Guid.NewGuid():N}";
        connection.Pooling = false;
        ConnectionString = connection.ConnectionString;

        using var context = CreateDbContext();
        try
        {
            context.Database.Migrate();
        }
        catch
        {
            // A failed migration can leave a partially created database behind.
            try
            {
                context.Database.EnsureDeleted();
            }
            catch
            {
                // Preserve the original setup failure.
            }
            throw;
        }
    }

    public HouseholdSuppliesDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<HouseholdSuppliesDbContext>()
            .UseAzureSql(ConnectionString)
            .Options);

    public void Reset()
    {
        using var context = CreateDbContext();
        // Migration tests can leave the database at an earlier migration or remove it entirely.
        context.Database.Migrate();
        context.ShoppingListItems.ExecuteDelete();
        context.Items.ExecuteDelete();
        context.ShoppingLists.ExecuteDelete();
        context.Places.ExecuteDelete();
        context.Users.ExecuteDelete();
    }

    public void Dispose()
    {
        using var context = CreateDbContext();
        context.Database.EnsureDeleted();
    }
}
