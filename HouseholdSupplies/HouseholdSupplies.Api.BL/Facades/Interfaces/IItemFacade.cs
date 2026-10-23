using HouseholdSupplies.Common.BL;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Api.BL.Facades.Interfaces;

public interface IItemFacade : IAppFacade
{
    IEnumerable<ItemListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? placeId = null,
        string categoryName = nameof(Category),
        int page = 1,
        int pageSize = 25);
    ItemDetailModel? GetById(Guid id);
    Guid CreateOrUpdate(ItemDetailModel model, IList<string> userRoles, string? ownerId = null);
    Guid Create(ItemDetailModel itemModel, IList<string> userRoles, string? ownerId = null);
    Guid? Update(ItemDetailModel itemModel, IList<string> userRoles, string? ownerId = null);
    void Delete(Guid id, IList<string> userRoles, string? ownerId = null);
}