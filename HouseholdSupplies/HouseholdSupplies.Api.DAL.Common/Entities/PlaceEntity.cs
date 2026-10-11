namespace HouseholdSupplies.Api.DAL.Common.Entities;

public record PlaceEntity : EntityBase
{
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }

    public ICollection<ItemEntity> Items { get; set; } = [];

    public Guid AuthorId { get; set; }
    public UserEntity? Author { get; set; }
}
