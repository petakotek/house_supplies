using HouseholdSupplies.Api.DAL.Common.Entities.Interfaces;
using HouseholdSupplies.Api.DAL.Common.Repositories;

namespace HouseholdSupplies.Api.BL.Facades;

public class FacadeBase<TRepository, TEntity>(
    TRepository repository)
    where TRepository : IApiRepository<TEntity>
    where TEntity : IEntity
{
    protected void ThrowIfWrongOwnerAndNotAdmin(Guid? id, IList<string> userRoles, string? ownerId)
    {
        if (id is not null
            && userRoles is not null
            && ownerId is not null)
        {
            throw new UnauthorizedAccessException();
        }
    }
}