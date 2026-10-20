using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.ShoppingListItem;

namespace HouseholdSupplies.Api.BL.Facades;

public class ShoppingListItemFacade(IShoppingListItemRepository shoppingListItemRepository, ShoppingListItemMapper mapper)
    : FacadeBase<IShoppingListItemRepository,
        ShoppingListItemEntity,
        ShoppingListItemDetailModel,
        ShoppingListItemListModel,
        ShoppingListItemMapper>(shoppingListItemRepository, mapper), IShoppingListItemFacade
{
    private readonly ShoppingListItemMapper _mapper = mapper;
    private readonly IShoppingListItemRepository _shoppingListItemRepository = shoppingListItemRepository;

    public IEnumerable<ShoppingListItemListModel> GetAll(
        bool? isBought = null
        )
    {
        IQueryable<ShoppingListItemEntity> query = _shoppingListItemRepository.GetAll();

        if (isBought.HasValue)
        {
            query = query.Where(item => item.IsBought == isBought.Value);
        }

        IEnumerable<ShoppingListItemEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }
}