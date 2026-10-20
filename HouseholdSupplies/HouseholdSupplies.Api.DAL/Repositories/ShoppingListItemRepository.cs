using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;

namespace HouseholdSupplies.Api.DAL.Repositories;

public class ShoppingListItemRepository : RepositoryBase<ShoppingListItemEntity>, IShoppingListItemRepository
{
    public ShoppingListItemRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}