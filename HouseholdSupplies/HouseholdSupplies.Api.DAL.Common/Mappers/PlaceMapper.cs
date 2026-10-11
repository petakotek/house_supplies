using HouseholdSupplies.Api.DAL.Common.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Common.Mappers;

[Mapper]
public partial class PlaceMapper
{
    [MapperIgnoreSource(nameof(PlaceEntity.Id))]
    public partial void UpdateEntity(PlaceEntity source, PlaceEntity target);
}
