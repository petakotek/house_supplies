using HouseholdSupplies.Common.Models.Item;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Common.Models.Place;

public record PlaceDetailModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public ICollection<ItemListModel> Items { get; set; } = [];
    public UserListModel? Author { get; set; }
}