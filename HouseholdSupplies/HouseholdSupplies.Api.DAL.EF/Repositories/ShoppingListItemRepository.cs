using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class ShoppingListItemRepository : RepositoryBase<ShoppingListItemEntity>, IShoppingListItemRepository
{
    public ShoppingListItemRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}