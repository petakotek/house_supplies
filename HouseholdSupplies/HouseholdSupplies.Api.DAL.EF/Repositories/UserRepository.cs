using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class UserRepository : RepositoryBase<UserEntity>
{
    public UserRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}