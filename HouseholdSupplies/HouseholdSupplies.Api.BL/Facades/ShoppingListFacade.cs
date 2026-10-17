using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.ShoppingList;

namespace HouseholdSupplies.Api.BL.Facades;

public class ShoppingListFacade(IShoppingListRepository shoppingListRepository, ShoppingListMapper mapper)
    : FacadeBase<IShoppingListRepository, ShoppingListEntity>(shoppingListRepository), IShoppingListFacade
{
    private readonly ShoppingListMapper _mapper = mapper;
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;

    public IEnumerable<ShoppingListListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? authorId = null
        )
    {
        IQueryable<ShoppingListEntity> query = _shoppingListRepository.GetAll();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(list => list.Description != null && (
                list.Name.Contains(searchQuery) ||
                list.Description.Contains(searchQuery) ||
                list.Shop.Contains(searchQuery)
                ));
        }

        if (authorId.HasValue)
        {
            query = query.Where(list => list.AuthorId == authorId.Value);
        }

        query = ascending ? query.OrderBy(list => list.Name) : query.OrderByDescending(list => list.Name);
        
        IEnumerable<ShoppingListEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }

    public ShoppingListDetailModel? GetById(Guid id)
    {
        var shoppingListEntity = _shoppingListRepository.GetById(id);
        if (shoppingListEntity == null)
        {
            return null;
        }
        return _mapper.ToDetailModel(shoppingListEntity);
    }

    public Guid CreateOrUpdate(ShoppingListDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        return _shoppingListRepository.Exists(model.Id)
            ? Update(model, userRoles, ownerId)!.Value
            : Create(model, userRoles, ownerId);
    }

    public Guid Create(ShoppingListDetailModel shoppingListModel, IList<string> userRoles, string? ownerId = null)
    {
        var itemEntity = _mapper.ToEntity(shoppingListModel, ownerId);
        return _shoppingListRepository.Insert(itemEntity);
    }

    public Guid? Update(ShoppingListDetailModel shoppingListModel, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(shoppingListModel.Id, userRoles, ownerId);

        var itemEntity = _mapper.ToEntity(shoppingListModel, ownerId);
        return _shoppingListRepository.Update(itemEntity);
    }

    public void Delete(Guid id, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(id, userRoles, ownerId);
        _shoppingListRepository.Remove(id);
    }
}