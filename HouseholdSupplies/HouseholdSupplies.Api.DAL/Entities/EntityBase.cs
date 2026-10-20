using HouseholdSupplies.Api.DAL.Entities.Interfaces;

namespace HouseholdSupplies.Api.DAL.Entities;

public abstract record EntityBase : IEntity
{
    public required Guid Id { get; init; }
    public string? OwnerId { get; set; }
}