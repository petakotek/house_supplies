using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace HouseholdSupplies.Api.DAL.Tests;

public sealed class SqlServerMigrationTests
{
    [Fact]
    public void Full_Idempotent_Script_Creates_SqlServer_Tables_With_Guid_Keys()
    {
        // Arrange: script generation never opens this connection.
        using var context = CreateDbContext();
        var migrator = context.GetService<IMigrator>();

        // Act
        var script = migrator.GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);

        // Assert: this is the same complete migration path used by the deployment pipeline.
        Assert.Contains("IF NOT EXISTS", script);
        Assert.Contains("[__EFMigrationsHistory]", script);
        foreach (var table in new[] { "Users", "Places", "Items", "ShoppingLists", "ShoppingListItems" })
        {
            Assert.Contains($"CREATE TABLE [{table}]", script);
            Assert.Contains($"CONSTRAINT [PK_{table}] PRIMARY KEY ([Id])", script);
        }

        Assert.Equal(5, System.Text.RegularExpressions.Regex.Matches(
            script, @"\[Id\] uniqueidentifier NOT NULL").Count);
        Assert.DoesNotMatch(@"(?i)\]\s+(TEXT|INTEGER|REAL)\b", script);
        Assert.Contains("[ExpirationDate] datetime2 NULL", script);
        Assert.Contains("[IsBought] bit NOT NULL", script);
        Assert.Contains("[Amount] float NOT NULL", script);
    }

    [Fact]
    public void Production_Migrations_Create_Guid_Keys_With_Valid_Column_Types()
    {
        // Arrange
        using var context = CreateDbContext();
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        // Act
        var operations = migrationsAssembly.Migrations.Values
            .Select(migration => migrationsAssembly.CreateMigration(
                migration, context.Database.ProviderName!))
            .SelectMany(migration => migration.UpOperations)
            .ToList();

        // Assert: a later provider-conversion migration cannot repair an invalid initial table.
        var tables = operations.OfType<CreateTableOperation>().ToList();
        Assert.Equal(5, tables.Count);
        foreach (var table in tables)
        {
            var id = Assert.Single(table.Columns, column => column.Name == "Id");
            Assert.Equal(typeof(Guid), id.ClrType);
            Assert.Equal("uniqueidentifier", id.ColumnType);
            Assert.False(id.IsNullable);

            foreach (var foreignKey in table.ForeignKeys)
            {
                foreach (var columnName in foreignKey.Columns)
                {
                    var column = Assert.Single(table.Columns, column => column.Name == columnName);
                    Assert.Equal(typeof(Guid), column.ClrType);
                    Assert.Equal("uniqueidentifier", column.ColumnType);
                }
            }
        }

        Assert.DoesNotContain(operations.OfType<AlterColumnOperation>(), column =>
            column.OldColumn.ColumnType is "TEXT" or "INTEGER" or "REAL");
    }

    [Fact]
    public void SqlServer_Model_Has_No_Pending_Migration_Changes()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        Assert.False(hasPendingChanges);
    }

    private static HouseholdSuppliesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HouseholdSuppliesDbContext>()
            .UseAzureSql("Server=localhost;Database=MigrationScriptTests;Integrated Security=True;TrustServerCertificate=True")
            .Options;
        return new HouseholdSuppliesDbContext(options);
    }
}
