using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Common.Models.ShoppingListItem;

public record ShoppingListItemDetailModel : IWithId
{
    public Guid Id { get; init; }
    public required double Amount { get; set; }
    public bool IsBought { get; set; }
    public ItemEntity? Item { get; set; }
    public ShoppingListEntity? ShoppingList { get; set; }
}