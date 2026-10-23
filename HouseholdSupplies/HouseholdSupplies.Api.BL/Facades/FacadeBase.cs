using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.BL.Mappers.Interfaces;
using HouseholdSupplies.Api.DAL.Entities.Interfaces;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common;

namespace HouseholdSupplies.Api.BL.Facades;

public class FacadeBase<TRepository, TEntity, TDetailModel, TListModel, TMapper>(
    TRepository repository, TMapper mapper)
    where TRepository : IApiRepository<TEntity>
    where TEntity : IEntity
    where TDetailModel : class, IWithId
    where TMapper : IMapper<TEntity, TListModel, TDetailModel>
{
    protected void ThrowIfWrongOwnerAndNotAdmin(Guid? id, IList<string> userRoles, string? ownerId)
    {
        if (id is not null
            && userRoles is not null
            && !userRoles.Contains(UserRoles.Admin)
            && ownerId is not null
            && repository.GetById(id.Value)?.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException();
        }
    }

    public Guid Create(TDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        var itemEntity = mapper.ToEntity(model, ownerId);
        return repository.Insert(itemEntity);
    }
    
    public void Delete(Guid id, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(id, userRoles, ownerId);
        repository.Remove(id);
    }
    
    public Guid? Update(TDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(model.Id, userRoles, ownerId);

        var itemEntity = mapper.ToEntity(model, ownerId);
        return repository.Update(itemEntity);
    }
    
    public Guid CreateOrUpdate(TDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        return repository.Exists(model.Id)
            ? Update(model, userRoles, ownerId)!.Value
            : Create(model, userRoles, ownerId);
    }
    
    public TDetailModel? GetById(Guid id)
    {
        var itemEntity = repository.GetById(id);
        if (itemEntity == null)
        {
            return null;
        }
        return mapper.ToDetailModel(itemEntity);
    }
}
