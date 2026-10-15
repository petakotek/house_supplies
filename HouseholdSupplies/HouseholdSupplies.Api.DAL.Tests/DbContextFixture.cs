using HouseholdSupplies.Api.DAL.EF;
using HouseholdSupplies.Api.DAL.EF.Extensions;
using HouseholdSupplies.Api.DAL.EF.Installers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests;

public sealed class DbContextFixture : IDisposable
{
    private readonly string databasePath = Path.Combine(
        Path.GetTempPath(), $"household-supplies-tests-{Guid.NewGuid():N}.db");
    private string BackupPath => databasePath + ".backup";
    private readonly ServiceProvider serviceProvider;

    public DbContextFixture()
    {
        var services = new ServiceCollection();
        services.AddInstaller<ApiDALEFInstaller>(CreateConnectionString(databasePath));
        serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            dbContext.Database.EnsureCreated();
        }

        using var source = OpenConnection(databasePath);
        using var backup = OpenConnection(BackupPath);
        source.BackupDatabase(backup);
    }

    public IServiceScope CreateScope() => serviceProvider.CreateScope();

    public void RestoreDatabase()
    {
        using var backup = OpenConnection(BackupPath);
        using var destination = OpenConnection(databasePath);
        backup.BackupDatabase(destination);
    }

    private static string CreateConnectionString(string path) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = path,
            ForeignKeys = true,
            Pooling = false
        }.ToString();

    private static SqliteConnection OpenConnection(string path)
    {
        var connection = new SqliteConnection(CreateConnectionString(path));
        connection.Open();
        return connection;
    }

    public void Dispose()
    {
        try
        {
            using var scope = CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            dbContext.Database.EnsureDeleted();
            File.Delete(BackupPath);
        }
        finally
        {
            serviceProvider.Dispose();
        }
    }
}
