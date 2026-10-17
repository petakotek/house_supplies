using HouseholdSupplies.Common.BL;
using HouseholdSupplies.Common.Models.Place;

namespace HouseholdSupplies.Api.BL.Facades.Interfaces;

public interface IPlaceFacade : IAppFacade
{
    IEnumerable<PlaceListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? authorId = null,
        int page = 1,
        int pageSize = 25);
    PlaceDetailModel? GetById(Guid id);
    Guid CreateOrUpdate(PlaceDetailModel model, IList<string> userRoles, string? ownerId = null);
    Guid Create(PlaceDetailModel placeModel, IList<string> userRoles, string? ownerId = null);
    Guid? Update(PlaceDetailModel placeModel, IList<string> userRoles, string? ownerId = null);
    void Delete(Guid id, IList<string> userRoles, string? ownerId = null);
}