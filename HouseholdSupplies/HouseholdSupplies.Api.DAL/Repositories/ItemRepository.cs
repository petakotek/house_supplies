using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.Repositories;

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