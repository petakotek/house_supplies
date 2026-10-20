using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;
using HouseholdSupplies.Common.Models.Place;
using HouseholdSupplies.Common.Models.ShoppingList;
using HouseholdSupplies.Common.Models.ShoppingListItem;
using HouseholdSupplies.Common.Models.User;

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

    public static UserListModel? ToListModel(UserEntity? user) => user is null ? null : new()
    {
        Id = user.Id, Name = user.Name, ImageUrl = user.ImageUrl, Role = user.Role
    };

    public static PlaceListModel? ToListModel(PlaceEntity? place) => place is null ? null : new()
    {
        Id = place.Id, Name = place.Name, ImageUrl = place.ImageUrl
    };

    public static ItemListModel? ToListModel(ItemEntity? item) => item is null ? null : new()
    {
        Id = item.Id, Name = item.Name, ImageUrl = item.ImageUrl, Amount = item.Amount,
        Category = item.Category, Place = ToListModel(item.Place)
    };

    public static ShoppingListListModel? ToListModel(ShoppingListEntity? list) => list is null ? null : new()
    {
        Id = list.Id, Name = list.Name, Shop = list.Shop
    };

    public static ShoppingListItemListModel? ToListModel(ShoppingListItemEntity? listItem) => listItem is null ? null : new()
    {
        Id = listItem.Id, Amount = listItem.Amount, IsBought = listItem.IsBought, Item = ToListModel(listItem.Item)
    };
}

