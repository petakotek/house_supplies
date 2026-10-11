using HouseholdSupplies.Api.DAL.Common.Entities.Interfaces;

namespace HouseholdSupplies.Api.DAL.Common.Repositories;

public interface IApiRepository<TEntity> where TEntity : IEntity
{
    IList<TEntity> GetAll();
    TEntity? GetById(Guid id);
    Guid Insert(TEntity entity);
    Guid? Update(TEntity entity);
    void Remove(Guid id);
    bool Exists(Guid id);
}
