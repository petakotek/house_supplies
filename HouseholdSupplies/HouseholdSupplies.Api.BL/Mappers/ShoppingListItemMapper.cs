using HouseholdSupplies.Api.BL.Mappers.Interfaces;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Common.Models.ShoppingList;
using HouseholdSupplies.Common.Models.ShoppingListItem;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.BL.Mappers;

[Mapper]
public partial class ShoppingListItemMapper : IMapper<ShoppingListItemEntity, ShoppingListItemListModel, ShoppingListItemDetailModel>
{
    [MapperIgnoreSource(nameof(ShoppingListItemEntity.ShoppingListId))]
    [MapperIgnoreSource(nameof(ShoppingListItemEntity.ShoppingList))]
    [MapperIgnoreSource(nameof(ShoppingListItemEntity.ItemId))]
    public partial ShoppingListItemListModel ToListModel(ShoppingListItemEntity entity);
    public partial ShoppingListItemDetailModel ToDetailModel(ShoppingListItemEntity entity);
    [MapperIgnoreTarget(nameof(ShoppingListItemEntity.Item))]
    [MapperIgnoreTarget(nameof(ShoppingListItemEntity.ShoppingList))]
    public partial ShoppingListItemEntity ToEntity(ShoppingListItemDetailModel model, string? ownerId = null);
    public partial IEnumerable<ShoppingListItemListModel> ToListModels(IEnumerable<ShoppingListItemEntity> entities);
}