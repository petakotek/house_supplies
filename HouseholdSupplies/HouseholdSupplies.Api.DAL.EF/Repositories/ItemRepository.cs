using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class ItemRepository : RepositoryBase<ItemEntity>, IItemRepository
{
    public ItemRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
    
    public override ItemEntity? GetById(Guid id)
    {
        return dbContext.Items
            .Include(item => item.Author)
            .Include(item => item.Place)
            .SingleOrDefault(item => item.Id == id);
    }
}