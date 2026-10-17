using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.ShoppingList;

namespace HouseholdSupplies.Api.BL.Facades;

public class ShoppingListFacade(IShoppingListRepository shoppingListRepository, ShoppingListMapper mapper)
    : FacadeBase<IShoppingListRepository, ShoppingListEntity, ShoppingListDetailModel, ShoppingListListModel, ShoppingListMapper>(shoppingListRepository, mapper), IShoppingListFacade
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
}