using HouseholdSupplies.Api.DAL.EF;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Tests;

public abstract class DalTestBase : IDisposable, IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture fixture;
    private readonly List<IServiceScope> scopes = [];
    private bool disposed;

    protected DalTestBase(DbContextFixture fixture)
    {
        this.fixture = fixture;
    }

    protected HouseholdSuppliesDbContext CreateDbContext()
    {
        return CreateScope().ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
    }

    protected IServiceScope CreateScope()
    {
        var scope = fixture.CreateScope();
        scopes.Add(scope);
        return scope;
    }

    protected void Seed(params object[] entities)
    {
        using var context = CreateDbContext();
        context.AddRange(entities);
        context.SaveChanges();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        try
        {
            foreach (var scope in scopes)
            {
                scope.Dispose();
            }
        }
        finally
        {
            fixture.RestoreDatabase();
        }
    }
}
