using HouseholdSupplies.Api.DAL.Extensions;
using HouseholdSupplies.Api.DAL.Installers;
using HouseholdSupplies.Api.DAL.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests;

public sealed class DbContextFixture : IDisposable
{
    private readonly SqlServerTestDatabase database;
    private readonly ServiceProvider serviceProvider;

    public DbContextFixture()
    {
        database = new SqlServerTestDatabase();
        try
        {
            var services = new ServiceCollection();
            services.AddInstaller<ApiDALInstaller>(database.ConnectionString);
            serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        }
        catch
        {
            database.Dispose();
            throw;
        }
    }

    public IServiceScope CreateScope() => serviceProvider.CreateScope();

    public void ResetDatabase() => database.Reset();

    public void Dispose()
    {
        try
        {
            serviceProvider.Dispose();
        }
        finally
        {
            database.Dispose();
        }
    }
}
