using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Extensions;
using HouseholdSupplies.Api.App.Models;
using Microsoft.AspNetCore.Mvc;
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
                return Results.BadRequest(new ApiErrorResponse(UserEndpointsResources.Paging));

            Role? roleFilter = null;
            if (role is not null)
            {
                if (!Enum.TryParse<Role>(role, true, out var parsedRole) || !Enum.IsDefined(parsedRole))
                    return Results.BadRequest(new ApiErrorResponse(UserEndpointsResources.Role));
                roleFilter = parsedRole;
            }
            return Results.Ok(users.GetAll(searchQuery, ascending, roleFilter, page, pageSize));
        })
            .ProducesResponse<IEnumerable<UserListModel>>(200, "A filtered, sorted page of users.")
            .ProducesResponse<ApiErrorResponse>(400, "Invalid pagination, role filter, or query parameter format.");

        endpoints.MapGet("{id:guid}", (Guid id, IUserFacade users) =>
            users.GetById(id) is { } user ? Results.Ok(user) : Results.NotFound())
            .ProducesResponse<UserListModel>(200, "The user's name, photo URL, and role.")
            .ProducesResponse(404, "The user does not exist.");

        endpoints.MapPost("", (UserListModel user, IUserFacade users) =>
            Results.Ok(users.Create(user))).AddEndpointFilter<ValidationFilter<UserListModel>>()
            .ProducesResponse<Guid>(200, "The created user ID.")
            .ProducesResponse<ApiErrorResponse>(400, "The request body is missing or malformed.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The user failed input validation.", "application/problem+json");

        endpoints.MapPut("", (UserListModel user, IUserFacade users) =>
        {
            var id = users.Update(user);
            return id.HasValue ? Results.Ok(id.Value) : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<UserListModel>>()
            .ProducesResponse<Guid>(200, "The updated user ID.")
            .ProducesResponse<ApiErrorResponse>(400, "The request body is missing or malformed.")
            .ProducesResponse(404, "The user does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The user failed input validation.", "application/problem+json");

        endpoints.MapDelete("{id:guid}", (Guid id, IUserFacade users) =>
            users.Delete(id) ? Results.Ok() : Results.NotFound())
            .ProducesResponse(200, "The user was deleted.")
            .ProducesResponse(404, "The user does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The user is referenced by existing records and cannot be deleted.", "application/problem+json");

        return endpointRouteBuilder;
    }
}
