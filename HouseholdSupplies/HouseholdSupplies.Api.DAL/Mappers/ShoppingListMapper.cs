using HouseholdSupplies.Api.DAL.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Mappers;

[Mapper]
public partial class ShoppingListMapper
{
    [MapperIgnoreSource(nameof(ShoppingListEntity.Id))]
    public partial void UpdateEntity(ShoppingListEntity source, ShoppingListEntity target);
}