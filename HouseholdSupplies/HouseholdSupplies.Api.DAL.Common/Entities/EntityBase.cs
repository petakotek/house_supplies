using HouseholdSupplies.Api.DAL.Common.Entities.Interfaces;

namespace HouseholdSupplies.Api.DAL.Common.Entities;

public abstract record EntityBase : IEntity
{
    public required Guid Id { get; init; }
}