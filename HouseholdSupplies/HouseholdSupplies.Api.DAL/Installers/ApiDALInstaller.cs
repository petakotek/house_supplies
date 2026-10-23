using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Installers;

public class ApiDALInstaller : ApiDALInstallerBase
{
    public void Install(IServiceCollection serviceCollection, string connectionString)
    {
        base.Install(serviceCollection);

        serviceCollection.AddDbContext<HouseholdSuppliesDbContext>(options => options.UseAzureSql(connectionString));
        
        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<ApiDALInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IApiRepository<>)))
                .AsMatchingInterface()
                .WithScopedLifetime());
    }
}