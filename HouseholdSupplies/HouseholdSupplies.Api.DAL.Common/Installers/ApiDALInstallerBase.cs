using HouseholdSupplies.Api.DAL.Common.Mappers;
using HouseholdSupplies.Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.DAL.Common.Installers;

public abstract class ApiDALInstallerBase : IInstaller
{
    public virtual void Install(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ItemMapper>();
        serviceCollection.AddSingleton<ShoppingListMapper>();
        serviceCollection.AddSingleton<ShoppingListItemMapper>();
        serviceCollection.AddSingleton<UserMapper>();
        serviceCollection.AddSingleton<PlaceMapper>();
    }
}