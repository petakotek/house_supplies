using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Common.Models.Place;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HouseholdSupplies.Api.App.Endpoints;

public class PlaceEndpoints(IOptions<IdentityOptions> identityOptions) : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var placeEndpoints = endpointRouteBuilder.MapGroup("place").WithTags("place");

        placeEndpoints.MapGet("", (IPlaceFacade placeFacade) => placeFacade.GetAll());

        placeEndpoints.MapGet("{id:guid}",
            Results<Ok<PlaceDetailModel>, NotFound> (Guid id, IPlaceFacade placeFacade) =>
            {
                var place = placeFacade.GetById(id);
                return place is not null ? TypedResults.Ok(place) : TypedResults.NotFound();
            });

        var placeModifyingEndpoints = placeEndpoints.MapGroup("");

        placeModifyingEndpoints.MapPost("",
            (PlaceDetailModel place, IPlaceFacade placeFacade, IHttpContextAccessor httpContextAccessor) =>
            {
                var userId = GetUserId(httpContextAccessor);
                var userRoles = GetUserRoles(httpContextAccessor);

                return TypedResults.Ok(placeFacade.Create(place, userRoles, userId));
            }).AddEndpointFilter<ValidationFilter<PlaceDetailModel>>();

        placeModifyingEndpoints.MapPut("",
            Results<Ok<Guid>, NotFound, ForbidHttpResult> (PlaceDetailModel place, IPlaceFacade placeFacade,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var userid = GetUserId(httpContextAccessor);
                var userRoles = GetUserRoles(httpContextAccessor);

                try
                {
                    var updatedId = placeFacade.Update(place, userRoles, userid);
                    return updatedId is not null ? TypedResults.Ok(updatedId.Value) : TypedResults.NotFound();
                }
                catch (UnauthorizedAccessException)
                {
                    return TypedResults.Forbid();
                }
            }).AddEndpointFilter<ValidationFilter<PlaceDetailModel>>();
        
        placeModifyingEndpoints.MapPost("upsert", Results<Ok<Guid>, ProblemHttpResult> (PlaceDetailModel place, IPlaceFacade placeFacade, IHttpContextAccessor httpContextAccessor)
            =>
        {
            var userId = GetUserId(httpContextAccessor);
            var userRoles = GetUserRoles(httpContextAccessor);

            try
            {
                return TypedResults.Ok(placeFacade.CreateOrUpdate(place, userRoles, userId));
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: PlaceEndpointsResources.Upsert_Forbidden_Title,
                    detail: string.Format(PlaceEndpointsResources.Upsert_Forbidden_Detail, place.Id));
            }
        }).AddEndpointFilter<ValidationFilter<PlaceDetailModel>>();
        
        placeModifyingEndpoints.MapDelete("{id:guid}", Results<Ok, ForbidHttpResult> (Guid id, IPlaceFacade placeFacade, IHttpContextAccessor httpContextAccessor) =>
        {
            var userRoles = GetUserRoles(httpContextAccessor);
            var userId = GetUserId(httpContextAccessor);
            
            try
            {
                placeFacade.Delete(id, userRoles, userId);
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