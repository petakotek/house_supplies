using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.User;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.BL.Facades;

public class UserFacade(IUserRepository repository, UserMapper mapper) : IUserFacade
{
    public IEnumerable<UserListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Role? role = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = repository.GetAll().AsNoTracking();
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var search = searchQuery.ToLowerInvariant();
            query = query.Where(user => user.Name.ToLower().Contains(search));
        }

        if (role.HasValue)
            query = query.Where(user => user.Role == role.Value);

        var ordered = ascending ? query.OrderBy(user => user.Name) : query.OrderByDescending(user => user.Name);
        var pageQuery = ordered.ThenBy(user => user.Id).Skip((page - 1) * pageSize).Take(pageSize);
        return mapper.ToListModels(pageQuery).ToList();
    }

    public UserListModel? GetById(Guid id)
    {
        var entity = repository.GetById(id);
        return entity is null ? null : mapper.ToListModel(entity);
    }

    public Guid Create(UserListModel model)
        => repository.Insert(mapper.ToEntity(model with { Id = Guid.NewGuid() }));

    public Guid? Update(UserListModel model)
    {
        var entity = repository.GetById(model.Id);
        if (entity is null)
            return null;

        // Update the tracked entity without replacing its ownership or relationships.
        mapper.UpdateEntity(model, entity);
        return repository.Update(entity);
    }

    public bool Delete(Guid id)
    {
        if (!repository.Exists(id))
            return false;

        repository.Remove(id);
        return true;
    }
}
