using HouseholdSupplies.Api.DAL.Common.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Common.Mappers;

[Mapper]
public partial class ShoppingListMapper
{
    [MapperIgnoreSource(nameof(ShoppingListEntity.Id))]
    public partial void UpdateEntity(ShoppingListEntity source, ShoppingListEntity target);
}