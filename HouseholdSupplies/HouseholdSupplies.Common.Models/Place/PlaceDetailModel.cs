using HouseholdSupplies.Api.DAL.Common.Entities;

namespace HouseholdSupplies.Common.Models.Place;

public record PlaceDetailModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public ICollection<ItemEntity> Items { get; set; } = [];
    public UserEntity? Author { get; set; }
}