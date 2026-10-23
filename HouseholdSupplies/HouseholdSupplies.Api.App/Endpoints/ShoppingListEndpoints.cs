using HouseholdSupplies.Api.App.Extensions;
using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Models;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.ShoppingList;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HouseholdSupplies.Api.App.Endpoints;

public class ShoppingListEndpoints(IOptions<IdentityOptions> identityOptions) : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var shoppingListEndpoints = endpointRouteBuilder.MapGroup("shopping-list").WithTags("shopping-list")
            .AddEndpointFilter<PersistenceExceptionFilter>();

        shoppingListEndpoints.MapGet("", (IShoppingListFacade shoppingListFacade, string? searchQuery = null,
            bool ascending = true, Guid? authorId = null, int page = 1, int pageSize = 25) =>
        {
            if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
                return Results.BadRequest(new ApiErrorResponse(ShoppingListEndpointsResources.Paging));

            return Results.Ok(shoppingListFacade.GetAll(searchQuery, ascending, authorId, page, pageSize));
        })
            .ProducesResponse<IEnumerable<ShoppingListListModel>>(200, "A filtered, sorted page of shopping lists.")
            .ProducesResponse<ApiErrorResponse>(400, "Invalid pagination or query parameter format.");

        shoppingListEndpoints.MapGet("{id:guid}", (Guid id, IShoppingListFacade shoppingListFacade) =>
            shoppingListFacade.GetById(id) is { } shoppingList ? Results.Ok(shoppingList) : Results.NotFound())
            .ProducesResponse<ShoppingListDetailModel>(200, "The shopping list detail, including its author and entries.")
            .ProducesResponse(404, "The shopping list does not exist.");

        var shoppingListModifyingEndpoints = shoppingListEndpoints.MapGroup("");

        shoppingListModifyingEndpoints.MapPost("", (ShoppingListDetailModel shoppingList,
                IShoppingListFacade shoppingListFacade, IHttpContextAccessor httpContextAccessor) =>
            Results.Ok(shoppingListFacade.Create(shoppingList with { Id = Guid.NewGuid() },
                GetUserRoles(httpContextAccessor), GetUserId(httpContextAccessor))))
            .AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>()
            .ProducesResponse<Guid>(200, "The created shopping list ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The shopping list failed input validation.", "application/problem+json");

        shoppingListModifyingEndpoints.MapPut("", (ShoppingListDetailModel shoppingList,
            IShoppingListFacade shoppingListFacade, IShoppingListRepository shoppingLists,
            IHttpContextAccessor httpContextAccessor) =>
        {
            // Check existence before ownership so a missing record always returns 404.
            if (!shoppingLists.Exists(shoppingList.Id))
                return Results.NotFound();
            try
            {
                var updatedId = shoppingListFacade.Update(shoppingList, GetUserRoles(httpContextAccessor),
                    GetUserId(httpContextAccessor));
                return updatedId.HasValue ? Results.Ok(updatedId.Value) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>()
            .ProducesResponse<Guid>(200, "The updated shopping list ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this shopping list.", "application/problem+json")
            .ProducesResponse(404, "The shopping list does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The shopping list failed input validation.", "application/problem+json");

        shoppingListModifyingEndpoints.MapPost("save", (ShoppingListDetailModel shoppingList,
            IShoppingListFacade shoppingListFacade, IHttpContextAccessor httpContextAccessor) =>
        {
            try
            {
                if (shoppingList.Id == Guid.Empty)
                    shoppingList = shoppingList with { Id = Guid.NewGuid() };
                return Results.Ok(shoppingListFacade.CreateOrUpdate(shoppingList, GetUserRoles(httpContextAccessor),
                    GetUserId(httpContextAccessor)));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>()
            .ProducesResponse<Guid>(200, "The created or updated shopping list ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this shopping list.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The shopping list failed input validation.", "application/problem+json");

        shoppingListModifyingEndpoints.MapDelete("{id:guid}", (Guid id, IShoppingListFacade shoppingListFacade,
            IHttpContextAccessor httpContextAccessor) =>
        {
            if (shoppingListFacade.GetById(id) is null)
                return Results.NotFound();
            try
            {
                shoppingListFacade.Delete(id, GetUserRoles(httpContextAccessor), GetUserId(httpContextAccessor));
                return Results.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        })
            .ProducesResponse(200, "The shopping list was deleted.")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot delete this shopping list.", "application/problem+json")
            .ProducesResponse(404, "The shopping list does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json");

        return endpointRouteBuilder;
    }

    private static IResult Forbidden() =>
        Results.Problem(statusCode: StatusCodes.Status403Forbidden,
            title: ShoppingListEndpointsResources.Forbidden_Title, detail: ShoppingListEndpointsResources.Forbidden_Detail);
}
