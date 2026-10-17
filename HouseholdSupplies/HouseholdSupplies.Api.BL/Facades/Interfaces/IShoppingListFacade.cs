using HouseholdSupplies.Common.Models.ShoppingList;

namespace HouseholdSupplies.Api.BL.Facades.Interfaces;

public interface IShoppingListFacade
{
    IEnumerable<ShoppingListListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? authorId = null
        );
    ShoppingListDetailModel? GetById(Guid id);
    Guid CreateOrUpdate(ShoppingListDetailModel model, IList<string> userRoles, string? ownerId = null);
    Guid Create(ShoppingListDetailModel shoppingListModel, IList<string> userRoles, string? ownerId = null);
    Guid? Update(ShoppingListDetailModel shoppingListModel, IList<string> userRoles, string? ownerId = null);
    void Delete(Guid id, IList<string> userRoles, string? ownerId = null);
}