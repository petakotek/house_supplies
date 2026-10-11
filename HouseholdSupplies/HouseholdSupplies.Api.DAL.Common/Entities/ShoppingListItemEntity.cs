namespace HouseholdSupplies.Api.DAL.Common.Entities;

public record ShoppingListItemEntity : EntityBase
{
    public required double Amount { get; set; }
    public bool IsBought { get; set; }

    public Guid ShoppingListId { get; set; }
    public ShoppingListEntity? ShoppingList { get; set; }

    public Guid ItemId { get; set; }
    public ItemEntity? Item { get; set; }
}
