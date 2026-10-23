using HouseholdSupplies.Common;

namespace HouseholdSupplies.Api.DAL.Entities.Interfaces;

public interface IEntity : IWithId
{
    string? OwnerId { get; set; }
}