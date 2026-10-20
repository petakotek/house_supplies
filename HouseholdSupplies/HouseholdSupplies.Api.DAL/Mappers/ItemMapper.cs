using HouseholdSupplies.Api.DAL.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Mappers;

[Mapper]
public partial class ItemMapper
{
    [MapperIgnoreSource(nameof(ItemEntity.Id))]
    public partial void UpdateEntity(ItemEntity source, ItemEntity target);
}
