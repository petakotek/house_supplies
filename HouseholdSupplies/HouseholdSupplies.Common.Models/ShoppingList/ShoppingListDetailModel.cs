using HouseholdSupplies.Common.Models.ShoppingListItem;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Common.Models.ShoppingList;

public record ShoppingListDetailModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Shop { get; set; }
    public string? Description { get; set; }
    public ICollection<ShoppingListItemListModel> Items { get; set; } = [];
    public UserListModel? Author { get; set; }
}