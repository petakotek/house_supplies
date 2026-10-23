using HouseholdSupplies.Api.DAL.Entities.Interfaces;

namespace HouseholdSupplies.Api.DAL.Repositories.Interfaces;

public interface IApiRepository<TEntity> where TEntity : IEntity
{
    IQueryable<TEntity> GetAll();
    TEntity? GetById(Guid id);
    Guid Insert(TEntity entity);
    Guid? Update(TEntity entity);
    void Remove(Guid id);
    bool Exists(Guid id);
}
