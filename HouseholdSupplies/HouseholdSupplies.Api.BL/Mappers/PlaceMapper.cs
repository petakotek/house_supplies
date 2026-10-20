using HouseholdSupplies.Api.BL.Mappers.Interfaces;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Common.Models.Place;
using Riok.Mapperly.Abstractions;

namespace HouseholdSupplies.Api.BL.Mappers;

[Mapper]
public partial class PlaceMapper : IMapper<PlaceEntity, PlaceListModel, PlaceDetailModel>
{
    [MapperIgnoreSource(nameof(PlaceEntity.Description))]
    [MapperIgnoreSource(nameof(PlaceEntity.Author))]
    [MapperIgnoreSource(nameof(PlaceEntity.Items))]
    [MapperIgnoreSource(nameof(PlaceEntity.AuthorId))]
    public partial PlaceListModel ToListModel(PlaceEntity entity);
    [MapperIgnoreSource(nameof(PlaceEntity.AuthorId))]
    public partial PlaceDetailModel ToDetailModel(PlaceEntity entity);
    [MapperIgnoreTarget(nameof(PlaceEntity.Author))]
    [MapperIgnoreTarget(nameof(PlaceEntity.Items))]
    public partial PlaceEntity ToEntity(PlaceDetailModel model, string? ownerId = null);
    public partial IEnumerable<PlaceListModel> ToListModels(IEnumerable<PlaceEntity> entities);
}