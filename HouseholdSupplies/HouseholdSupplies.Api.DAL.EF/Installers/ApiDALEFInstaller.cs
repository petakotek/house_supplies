using HouseholdSupplies.Api.DAL.Common.Installers;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.EF.Installers;

public class ApiDALEFInstaller : ApiDALInstallerBase
{
    public void Install(IServiceCollection serviceCollection, string connectionString)
    {
        base.Install(serviceCollection);

        serviceCollection.AddDbContext<HouseholdSuppliesDbContext>(options => options.UseSqlite(connectionString));
        
        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<ApiDALEFInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IApiRepository<>)))
                .AsMatchingInterface()
                .WithScopedLifetime());
    }
}