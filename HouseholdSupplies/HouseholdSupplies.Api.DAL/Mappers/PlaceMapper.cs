using HouseholdSupplies.Api.DAL.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Mappers;

[Mapper]
public partial class PlaceMapper
{
    [MapperIgnoreSource(nameof(PlaceEntity.Id))]
    public partial void UpdateEntity(PlaceEntity source, PlaceEntity target);
}
