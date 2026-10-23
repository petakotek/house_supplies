using HouseholdSupplies.Common.Models.Item;
using HouseholdSupplies.Common.Models.ShoppingList;

namespace HouseholdSupplies.Common.Models.ShoppingListItem;

public record ShoppingListItemDetailModel : IWithId
{
    public Guid Id { get; init; }
    public required double Amount { get; set; }
    public bool IsBought { get; set; }
    public ItemListModel? Item { get; set; }
    public ShoppingListListModel? ShoppingList { get; set; }
}