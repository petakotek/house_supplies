using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Common.Models.ShoppingListItem;

public record ShoppingListItemListModel() : IWithId
{
    public Guid Id { get; init; }
    public Guid ItemId { get; set; }
    public ItemEntity? Item { get; set; }
    public required double Amount { get; set; }
    public bool IsBought { get; set; }
}