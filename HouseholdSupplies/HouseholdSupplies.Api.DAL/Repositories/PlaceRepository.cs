using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.DAL.Repositories;

public class PlaceRepository : RepositoryBase<PlaceEntity>, IPlaceRepository
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