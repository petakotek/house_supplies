using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Common.Installers;

public interface IInstaller
{
    void Install(IServiceCollection serviceCollection);
}