using HouseholdSupplies.Api.DAL.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection, string connectionString)
        where TInstaller : ApiDALInstaller, new()
    {
        var installer =  new TInstaller();
        installer.Install(serviceCollection, connectionString);
    }
}