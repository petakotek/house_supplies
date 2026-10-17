using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.Place;

namespace HouseholdSupplies.Api.BL.Facades;

public class PlaceFacade(IPlaceRepository placeRepository, PlaceMapper mapper)
    : FacadeBase<IPlaceRepository, PlaceEntity>(placeRepository), IPlaceFacade
{
    private readonly IPlaceRepository _placeRepository = placeRepository;
    private readonly PlaceMapper _mapper = mapper;

    public IEnumerable<PlaceListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? authorId = null,
        int page = 1,
        int pageSize = 25)
    {
        IQueryable<PlaceEntity> query = _placeRepository.GetAll();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(place => place.Description != null && (
                place.Name.Contains(searchQuery) ||
                place.Description.Contains(searchQuery)));
        }

        if (authorId.HasValue)
        {
            query = query.Where(place => place.AuthorId == authorId.Value);
        }

        query = ascending ? query.OrderBy(place => place.Name) : query.OrderByDescending(place => place.Name);

        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        IEnumerable<PlaceEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }

    public PlaceDetailModel? GetById(Guid id)
    {
        var placeEntity = _placeRepository.GetById(id);
        if (placeEntity == null)
        {
            return null;
        }
        return _mapper.ToDetailModel(placeEntity);
    }

    public Guid CreateOrUpdate(PlaceDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        return _placeRepository.Exists(model.Id)
            ? Update(model, userRoles, ownerId)!.Value
            : Create(model, userRoles, ownerId);
    }

    public Guid Create(PlaceDetailModel placeModel, IList<string> userRoles, string? ownerId = null)
    {
        var itemEntity = _mapper.ToEntity(placeModel, ownerId);
        return _placeRepository.Insert(itemEntity);
    }

    public Guid? Update(PlaceDetailModel placeModel, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(placeModel.Id, userRoles, ownerId);

        var itemEntity = _mapper.ToEntity(placeModel, ownerId);
        return _placeRepository.Update(itemEntity);
    }

    public void Delete(Guid id, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(id, userRoles, ownerId);
        _placeRepository.Remove(id);
    }
}