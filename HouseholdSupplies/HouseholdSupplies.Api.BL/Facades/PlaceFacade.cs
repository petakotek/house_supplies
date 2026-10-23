using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.Place;

namespace HouseholdSupplies.Api.BL.Facades;

public class PlaceFacade(IPlaceRepository placeRepository, PlaceMapper mapper)
    : FacadeBase<IPlaceRepository, PlaceEntity, PlaceDetailModel, PlaceListModel, PlaceMapper>(placeRepository, mapper), IPlaceFacade
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
            var normalizedSearchQuery = searchQuery.ToLowerInvariant();

            query = query.Where(place =>
                place.Name.ToLower().Contains(normalizedSearchQuery) ||
                (place.Description != null && place.Description.ToLower().Contains(normalizedSearchQuery)));
        }

        if (authorId.HasValue)
        {
            query = query.Where(place => place.AuthorId == authorId.Value);
        }

        var orderedQuery = ascending ? query.OrderBy(place => place.Name) : query.OrderByDescending(place => place.Name);
        query = orderedQuery.ThenBy(place => place.Id);

        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        IEnumerable<PlaceEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }
}