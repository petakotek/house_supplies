using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class ShoppingListItemRepository : RepositoryBase<ShoppingListItemEntity>
{
    public ShoppingListItemRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}