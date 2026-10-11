using HouseholdSupplies.Api.DAL.Common.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Common.Mappers;

[Mapper]
public partial class ShoppingListItemMapper
{
    [MapperIgnoreSource(nameof(ShoppingListItemEntity.Id))]
    public partial void UpdateEntity(ShoppingListItemEntity source, ShoppingListItemEntity target);
}