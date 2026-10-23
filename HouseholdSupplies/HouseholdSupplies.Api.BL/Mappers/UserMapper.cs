using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Common.Models.User;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.BL.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(UserEntity.OwnerId))]
    [MapperIgnoreSource(nameof(UserEntity.Items))]
    [MapperIgnoreSource(nameof(UserEntity.Places))]
    [MapperIgnoreSource(nameof(UserEntity.ShoppingLists))]
    public partial UserListModel ToListModel(UserEntity entity);

    public partial IQueryable<UserListModel> ToListModels(IQueryable<UserEntity> entities);

    [MapperIgnoreTarget(nameof(UserEntity.OwnerId))]
    [MapperIgnoreTarget(nameof(UserEntity.Items))]
    [MapperIgnoreTarget(nameof(UserEntity.Places))]
    [MapperIgnoreTarget(nameof(UserEntity.ShoppingLists))]
    public partial UserEntity ToEntity(UserListModel model);

    [MapperIgnoreSource(nameof(UserListModel.Id))]
    [MapperIgnoreTarget(nameof(UserEntity.Id))]
    [MapperIgnoreTarget(nameof(UserEntity.OwnerId))]
    [MapperIgnoreTarget(nameof(UserEntity.Items))]
    [MapperIgnoreTarget(nameof(UserEntity.Places))]
    [MapperIgnoreTarget(nameof(UserEntity.ShoppingLists))]
    public partial void UpdateEntity(UserListModel model, UserEntity entity);
}
