using HouseholdSupplies.Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInstaller<TInstaller>(this IServiceCollection serviceCollection)
        where TInstaller : IInstaller, new()
    {
        var installer = new TInstaller();
        installer.Install(serviceCollection);
    }
}