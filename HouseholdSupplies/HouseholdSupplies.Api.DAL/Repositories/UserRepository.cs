using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;

namespace HouseholdSupplies.Api.DAL.Repositories;

public class UserRepository : RepositoryBase<UserEntity>, IUserRepository
{
    public UserRepository(HouseholdSuppliesDbContext dbContext) : base(dbContext)
    {
    }
}