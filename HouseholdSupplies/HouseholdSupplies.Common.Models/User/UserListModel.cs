using HouseholdSupplies.Common.Enums;

namespace HouseholdSupplies.Common.Models.User;

public record UserListModel() : IWithId
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public required Role Role { get; set; }
}