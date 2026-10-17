namespace HouseholdSupplies.Common.Models.Place;

public record PlaceListModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
}