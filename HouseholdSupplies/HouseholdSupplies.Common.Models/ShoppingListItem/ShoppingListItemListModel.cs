using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Common.Models.ShoppingListItem;

public record ShoppingListItemListModel() : IWithId
{
    public Guid Id { get; init; }
    public ItemListModel? Item { get; set; }
    public required double Amount { get; set; }
    public bool IsBought { get; set; }
}