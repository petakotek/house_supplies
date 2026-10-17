using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.DAL.Common.Entities;

public record ItemEntity : EntityBase
{
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public required double Amount { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public required Category Category { get; set; }
    public string? Description { get; set; }
    
    public Guid AuthorId { get; set; }
    public  UserEntity? Author { get; set; }
    

    public Guid PlaceId { get; set; }
    public PlaceEntity? Place { get; set; }

    
    public ICollection<ShoppingListItemEntity> ShoppingListItems { get; set; } = [];
}
