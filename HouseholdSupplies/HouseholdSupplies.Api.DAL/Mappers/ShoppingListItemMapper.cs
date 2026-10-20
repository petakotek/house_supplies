using HouseholdSupplies.Api.DAL.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Mappers;

[Mapper]
public partial class ShoppingListItemMapper
{
    [MapperIgnoreSource(nameof(ShoppingListItemEntity.Id))]
    public partial void UpdateEntity(ShoppingListItemEntity source, ShoppingListItemEntity target);
}