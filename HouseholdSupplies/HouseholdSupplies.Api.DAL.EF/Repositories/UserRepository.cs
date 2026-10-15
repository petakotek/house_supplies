using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;

namespace HouseholdSupplies.Api.DAL.EF.Repositories;

public class UserRepository : RepositoryBase<UserEntity>, IUserRepository
{
    public UserRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}