using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Place;

namespace HouseholdSupplies.Common.Models.Item;

public record ItemListModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ImageUrl { get; set; }
    public required double Amount { get; set; }
    public required Category Category { get; set; }
    public PlaceListModel? Place { get; set; }
}