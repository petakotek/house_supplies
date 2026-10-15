using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.DAL.Tests;

internal sealed class TestDataGraph
{
    public UserEntity User { get; }
    public PlaceEntity Place { get; }
    public ItemEntity Item { get; }
    public ShoppingListEntity ShoppingList { get; }
    public ShoppingListItemEntity Entry { get; }

    public TestDataGraph()
    {
        User = new UserEntity
        {
            Id = Guid.NewGuid(), Name = "Test user", ImageUrl = "user.png", Role = Role.User
        };
        Place = new PlaceEntity
        {
            Id = Guid.NewGuid(), Name = "Pantry", ImageUrl = "pantry.png",
            Description = "Kitchen storage", AuthorId = User.Id
        };
        Item = new ItemEntity
        {
            Id = Guid.NewGuid(), Name = "Rice", ImageUrl = "rice.png", Amount = 1.5,
            ExpirationDate = new DateTime(2027, 6, 30), Category = Category.Food,
            Description = "Brown rice", AuthorId = User.Id, PlaceId = Place.Id
        };
        ShoppingList = new ShoppingListEntity
        {
            Id = Guid.NewGuid(), Name = "Weekly shopping", Shop = "Supermarket",
            Description = "Weekend supplies", AuthorId = User.Id
        };
        Entry = new ShoppingListItemEntity
        {
            Id = Guid.NewGuid(), Amount = 2.5, IsBought = false,
            ShoppingListId = ShoppingList.Id, ItemId = Item.Id
        };
    }

    public object[] Entities => [User, Place, Item, ShoppingList, Entry];
}
