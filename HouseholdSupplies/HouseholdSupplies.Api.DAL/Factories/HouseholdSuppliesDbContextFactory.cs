using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HouseholdSupplies.Api.DAL.Factories;

public class HouseholdSuppliesDbContextFactory : IDesignTimeDbContextFactory<HouseholdSuppliesDbContext>
{
    public HouseholdSuppliesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<HouseholdSuppliesDbContextFactory>(optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HouseholdSuppliesDbContext>();
        optionsBuilder.UseAzureSql(configuration.GetConnectionString("DefaultConnection"));
        return new HouseholdSuppliesDbContext(optionsBuilder.Options);
    }
}