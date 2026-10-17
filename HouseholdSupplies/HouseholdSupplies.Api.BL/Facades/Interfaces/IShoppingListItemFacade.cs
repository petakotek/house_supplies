using HouseholdSupplies.Common.BL;
using HouseholdSupplies.Common.Models.ShoppingListItem;

namespace HouseholdSupplies.Api.BL.Facades.Interfaces;

public interface IShoppingListItemFacade : IAppFacade
{
    IEnumerable<ShoppingListItemListModel> GetAll(
        bool? isBought = null);
    ShoppingListItemDetailModel? GetById(Guid id);
    Guid CreateOrUpdate(ShoppingListItemDetailModel model, IList<string> userRoles, string? ownerId = null);
    Guid Create(ShoppingListItemDetailModel shoppingListItemModel, IList<string> userRoles, string? ownerId = null);
    Guid? Update(ShoppingListItemDetailModel shoppingListItemModel, IList<string> userRoles, string? ownerId = null);
    void Delete(Guid id, IList<string> userRoles, string? ownerId = null);
}