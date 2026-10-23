using HouseholdSupplies.Common.BL;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Api.BL.Facades.Interfaces;

public interface IUserFacade : IAppFacade
{
    IEnumerable<UserListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Role? role = null,
        int page = 1,
        int pageSize = 25);
    UserListModel? GetById(Guid id);
    Guid Create(UserListModel model);
    Guid? Update(UserListModel model);
    bool Delete(Guid id);
}
