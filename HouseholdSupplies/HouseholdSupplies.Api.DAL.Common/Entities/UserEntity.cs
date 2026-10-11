using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.DAL.Common.Entities;

public record UserEntity : EntityBase
{
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public required Role Role { get; set; }

    public ICollection<ItemEntity> Items { get; set; } = [];
    public ICollection<PlaceEntity> Places { get; set; } = [];
    public ICollection<ShoppingListEntity> ShoppingLists { get; set; } = [];
}
