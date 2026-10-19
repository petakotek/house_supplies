using HouseholdSupplies.Api.BL.Mappers.Interfaces;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Common.Models.ShoppingList;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.BL.Mappers;

[Mapper]
public partial class ShoppingListMapper : IMapper<ShoppingListEntity, ShoppingListListModel, ShoppingListDetailModel>
{
    public partial ShoppingListListModel ToListModel(ShoppingListEntity entity);
    public partial ShoppingListDetailModel ToDetailModel(ShoppingListEntity entity);
    public partial ShoppingListEntity ToEntity(ShoppingListDetailModel model, string? ownerId = null);
    public partial IEnumerable<ShoppingListListModel> ToListModels(IEnumerable<ShoppingListEntity> entities);
}