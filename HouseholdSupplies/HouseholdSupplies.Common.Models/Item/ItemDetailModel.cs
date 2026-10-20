using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Place;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Common.Models.Item;

public record ItemDetailModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ImageUrl { get; set; }
    public required double Amount { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public required Category Category { get; set; }
    public string? Description { get; set; }

    public UserListModel? Author { get; set; }
    public PlaceListModel? Place { get; set; }
}