namespace HouseholdSupplies.Common.Models.ShoppingList;

public record ShoppingListListModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Shop { get; set; }
}