using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Common.Models.ShoppingList;

public record ShoppingListDetailModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Shop { get; set; }
    public string? Description { get; set; }
    public ICollection<ShoppingListItemEntity> Items { get; set; } = [];
    public UserEntity? Author { get; set; }
}