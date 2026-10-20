using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Common.Models.ShoppingList;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HouseholdSupplies.Api.App.Endpoints;

public class ShoppingListEndpoints(IOptions<IdentityOptions> identityOptions) : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var shoppingListEndpoints = endpointRouteBuilder.MapGroup("shopping-list").WithTags("shopping-list");

        shoppingListEndpoints.MapGet("", (IShoppingListFacade shoppingListFacade) => shoppingListFacade.GetAll());

        shoppingListEndpoints.MapGet("{id:guid}",
            Results<Ok<ShoppingListDetailModel>, NotFound> (Guid id, IShoppingListFacade shoppingListFacade) =>
            {
                var shoppingList = shoppingListFacade.GetById(id);
                return shoppingList is not null ? TypedResults.Ok(shoppingList) : TypedResults.NotFound();
            });

        var shoppingListModifyingEndpoints = shoppingListEndpoints.MapGroup("");

        shoppingListModifyingEndpoints.MapPost("",
            (ShoppingListDetailModel shoppingList, IShoppingListFacade shoppingListFacade, IHttpContextAccessor httpContextAccessor) =>
            {
                var userId = GetUserId(httpContextAccessor);
                var userRoles = GetUserRoles(httpContextAccessor);

                return TypedResults.Ok(shoppingListFacade.Create(shoppingList, userRoles, userId));
            }).AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>();

        shoppingListModifyingEndpoints.MapPut("",
            Results<Ok<Guid>, NotFound, ForbidHttpResult> (ShoppingListDetailModel shoppingList, IShoppingListFacade shoppingListFacade,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var userId = GetUserId(httpContextAccessor);
                var userRoles = GetUserRoles(httpContextAccessor);

                try
                {
                    var updatedId = shoppingListFacade.Update(shoppingList, userRoles, userId);
                    return updatedId is not null ? TypedResults.Ok(updatedId.Value) : TypedResults.NotFound();
                }
                catch (UnauthorizedAccessException)
                {
                    return TypedResults.Forbid();
                }
            }).AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>();

        shoppingListModifyingEndpoints.MapPost("upsert", Results<Ok<Guid>, ProblemHttpResult> (ShoppingListDetailModel shoppingList, IShoppingListFacade shoppingListFacade, IHttpContextAccessor httpContextAccessor)
            =>
        {
            var userId = GetUserId(httpContextAccessor);
            var userRoles = GetUserRoles(httpContextAccessor);

            try
            {
                return TypedResults.Ok(shoppingListFacade.CreateOrUpdate(shoppingList, userRoles, userId));
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: ShoppingListEndpointsResources.Upsert_Forbidden_Title,
                    detail: string.Format(ShoppingListEndpointsResources.Upsert_Forbidden_Detail, shoppingList.Id));
            }
        }).AddEndpointFilter<ValidationFilter<ShoppingListDetailModel>>();

        shoppingListModifyingEndpoints.MapDelete("{id:guid}", Results<Ok, ForbidHttpResult> (Guid id, IShoppingListFacade shoppingListFacade, IHttpContextAccessor httpContextAccessor) =>
        {
            var userRoles = GetUserRoles(httpContextAccessor);
            var userId = GetUserId(httpContextAccessor);

            try
            {
                shoppingListFacade.Delete(id, userRoles, userId);
                return TypedResults.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Forbid();
            }
        });

        return endpointRouteBuilder;
    }
}
