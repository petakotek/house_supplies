using HouseholdSupplies.Api.DAL.Common.Entities;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.DAL.Common.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(UserEntity.Id))]
    public partial void UpdateEntity(UserEntity source, UserEntity target);
}
