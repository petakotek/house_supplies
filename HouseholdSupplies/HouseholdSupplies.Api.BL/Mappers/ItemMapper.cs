using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Common.Models.Item;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.BL.Mappers;

[Mapper]
public partial class ItemMapper
{
    [MapperIgnoreSource(nameof(ItemEntity.Description))]
    [MapperIgnoreSource(nameof(ItemEntity.ExpirationDate))]
    [MapperIgnoreSource(nameof(ItemEntity.Author))]
    [MapperIgnoreSource(nameof(ItemEntity.AuthorId))]
    [MapperIgnoreSource(nameof(ItemEntity.PlaceId))]
    [MapperIgnoreSource(nameof(ItemEntity.ShoppingListItems))]
    public partial ItemListModel ToListModel(ItemEntity entity);
    [MapperIgnoreSource(nameof(ItemEntity.ShoppingListItems))]
    public partial ItemDetailModel ToDetailModel(ItemEntity entity);
    public partial ItemEntity ToEntity(ItemDetailModel model, string? ownerId = null);
    public partial IEnumerable<ItemListModel> ToListModels(IEnumerable<ItemEntity> entities);
}