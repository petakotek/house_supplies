namespace HouseholdSupplies.Api.DAL.Common.Entities;

public record ShoppingListEntity : EntityBase
{
    public required string Name { get; set; }
    public required string Shop { get; set; }
    public string? Description { get; set; }

    public ICollection<ShoppingListItemEntity> Items { get; set; } = [];

    public Guid AuthorId { get; set; }
    public UserEntity? Author { get; set; }
}
