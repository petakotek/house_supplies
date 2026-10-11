using HouseholdSupplies.Api.DAL.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class ShoppingListRepository : RepositoryBase<ShoppingListEntity>
{
    public ShoppingListRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
    
    public override ShoppingListEntity? GetById(Guid id)
    {
        return dbContext.ShoppingLists
            .Include(list => list.Author)
            .Include(list => list.Items)
            .ThenInclude(entry => entry.Item)
            .SingleOrDefault(list => list.Id == id);
    }
    
    public override Guid? Update(ShoppingListEntity updated)
    {
        var existing = dbContext.ShoppingLists
            .SingleOrDefault(list => list.Id == updated.Id);

        if (existing is null)
        {
            return Guid.Empty;
        }

        existing.Name = updated.Name;
        existing.Shop = updated.Shop;
        existing.Description = updated.Description;

        dbContext.SaveChanges();

        return existing.Id;
    }
}