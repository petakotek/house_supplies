using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Api.App.Endpoints;

public class UserEndpoints : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var endpoints = endpointRouteBuilder.MapGroup("user").WithTags("user")
            .AddEndpointFilter<PersistenceExceptionFilter>();

        endpoints.MapGet("", (IUserFacade users, string? searchQuery = null, bool ascending = true, string? role = null,
            int page = 1, int pageSize = 25) =>
        {
            if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
                return Results.BadRequest(new { error = UserEndpointsResources.Paging });

            Role? roleFilter = null;
            if (role is not null)
            {
                if (!Enum.TryParse<Role>(role, true, out var parsedRole) || !Enum.IsDefined(parsedRole))
                    return Results.BadRequest(new { error = UserEndpointsResources.Role });
                roleFilter = parsedRole;
            }
            return Results.Ok(users.GetAll(searchQuery, ascending, roleFilter, page, pageSize));
        });

        endpoints.MapGet("{id:guid}", (Guid id, IUserFacade users) =>
            users.GetById(id) is { } user ? Results.Ok(user) : Results.NotFound());

        endpoints.MapPost("", (UserListModel user, IUserFacade users) =>
            Results.Ok(users.Create(user))).AddEndpointFilter<ValidationFilter<UserListModel>>();

        endpoints.MapPut("", (UserListModel user, IUserFacade users) =>
        {
            var id = users.Update(user);
            return id.HasValue ? Results.Ok(id.Value) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<UserListModel>>();

        endpoints.MapDelete("{id:guid}", (Guid id, IUserFacade users) =>
            users.Delete(id) ? Results.Ok() : Results.NotFound());

        return endpointRouteBuilder;
    }
}
