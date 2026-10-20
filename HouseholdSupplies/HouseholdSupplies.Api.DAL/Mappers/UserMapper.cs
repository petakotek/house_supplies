using HouseholdSupplies.Api.DAL.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(UserEntity.Id))]
    public partial void UpdateEntity(UserEntity source, UserEntity target);
}
