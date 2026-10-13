using HouseholdSupplies.Api.DAL.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class PlaceRepository : RepositoryBase<PlaceEntity>
{
    public PlaceRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
    
    public override PlaceEntity? GetById(Guid id)
    {
        return dbContext.Places
            .Include(place => place.Author)
            .Include(place => place.Items)
            .SingleOrDefault(place => place.Id == id);
    }
}