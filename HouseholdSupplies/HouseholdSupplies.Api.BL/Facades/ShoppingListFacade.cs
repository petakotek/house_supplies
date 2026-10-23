using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
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
        Guid? authorId = null,
        int page = 1,
        int pageSize = 25)
    {
        IQueryable<ShoppingListEntity> query = _shoppingListRepository.GetAll();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var normalizedSearchQuery = searchQuery.ToLowerInvariant();

            query = query.Where(list =>
                list.Name.ToLower().Contains(normalizedSearchQuery) ||
                (list.Description != null && list.Description.ToLower().Contains(normalizedSearchQuery)) ||
                list.Shop.ToLower().Contains(normalizedSearchQuery));
        }

        if (authorId.HasValue)
        {
            query = query.Where(list => list.AuthorId == authorId.Value);
        }

        var orderedQuery = ascending ? query.OrderBy(list => list.Name) : query.OrderByDescending(list => list.Name);
        query = orderedQuery.ThenBy(list => list.Id);

        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        IEnumerable<ShoppingListEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }
}