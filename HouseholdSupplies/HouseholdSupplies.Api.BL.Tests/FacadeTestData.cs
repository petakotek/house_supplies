using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Api.BL.Tests;

internal static class FacadeTestData
{
    public static UserEntity CreateUser() => new()
    {
        Id = Guid.NewGuid(), Name = "Alice", ImageUrl = "alice.png", Role = Role.User
    };

    public static ItemEntity CreateItem() => new()
    {
        Id = Guid.NewGuid(), Name = "Milk", ImageUrl = "milk.png", Amount = 2.5,
        Category = Category.Food, Description = "Fresh milk", ExpirationDate = new DateTime(2027, 1, 15),
        AuthorId = Guid.NewGuid(), PlaceId = Guid.NewGuid()
    };

    public static ShoppingListEntity CreateShoppingList() => new()
    {
        Id = Guid.NewGuid(), Name = "Weekly shopping", Shop = "Market",
        Description = "Groceries", AuthorId = Guid.NewGuid()
    };

    public static ShoppingListItemEntity CreateShoppingListItem()
    {
        var item = CreateItem();
        var list = CreateShoppingList();
        return new ShoppingListItemEntity
        {
            Id = Guid.NewGuid(), Amount = 3.5, IsBought = true,
            ItemId = item.Id, Item = item, ShoppingListId = list.Id, ShoppingList = list
        };
    }
}

