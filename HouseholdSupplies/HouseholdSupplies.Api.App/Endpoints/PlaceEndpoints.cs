using HouseholdSupplies.Api.App.Extensions;
using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Models;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.Place;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HouseholdSupplies.Api.App.Endpoints;

public class PlaceEndpoints(IOptions<IdentityOptions> identityOptions) : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var placeEndpoints = endpointRouteBuilder.MapGroup("place").WithTags("place")
            .AddEndpointFilter<PersistenceExceptionFilter>();

        placeEndpoints.MapGet("", (IPlaceFacade placeFacade, string? searchQuery = null, bool ascending = true,
            Guid? authorId = null, int page = 1, int pageSize = 25) =>
        {
            if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
                return Results.BadRequest(new ApiErrorResponse(PlaceEndpointsResources.Paging));

            return Results.Ok(placeFacade.GetAll(searchQuery, ascending, authorId, page, pageSize));
        })
            .ProducesResponse<IEnumerable<PlaceListModel>>(200, "A filtered, sorted page of places.")
            .ProducesResponse<ApiErrorResponse>(400, "Invalid pagination or query parameter format.");

        placeEndpoints.MapGet("{id:guid}", (Guid id, IPlaceFacade placeFacade) =>
            placeFacade.GetById(id) is { } place ? Results.Ok(place) : Results.NotFound())
            .ProducesResponse<PlaceDetailModel>(200, "The place detail, including its author and stored items.")
            .ProducesResponse(404, "The place does not exist.");

        var placeModifyingEndpoints = placeEndpoints.MapGroup("");

        placeModifyingEndpoints.MapPost("", (PlaceDetailModel place, IPlaceFacade placeFacade,
                IHttpContextAccessor httpContextAccessor) =>
            Results.Ok(placeFacade.Create(place with { Id = Guid.NewGuid() },
                GetUserRoles(httpContextAccessor), GetUserId(httpContextAccessor))))
            .AddEndpointFilter<ValidationFilter<PlaceDetailModel>>()
            .ProducesResponse<Guid>(200, "The created place ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The place failed input validation.", "application/problem+json");

        placeModifyingEndpoints.MapPut("", (PlaceDetailModel place, IPlaceFacade placeFacade, IPlaceRepository places,
            IHttpContextAccessor httpContextAccessor) =>
        {
            if (!places.Exists(place.Id))
                return Results.NotFound();
            try
            {
                var updatedId = placeFacade.Update(place, GetUserRoles(httpContextAccessor), GetUserId(httpContextAccessor));
                return updatedId.HasValue ? Results.Ok(updatedId.Value) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<PlaceDetailModel>>()
            .ProducesResponse<Guid>(200, "The updated place ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this place.", "application/problem+json")
            .ProducesResponse(404, "The place does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The place failed input validation.", "application/problem+json");

        placeModifyingEndpoints.MapPost("save", (PlaceDetailModel place, IPlaceFacade placeFacade,
            IHttpContextAccessor httpContextAccessor) =>
        {
            try
            {
                if (place.Id == Guid.Empty)
                    place = place with { Id = Guid.NewGuid() };
                return Results.Ok(placeFacade.CreateOrUpdate(place, GetUserRoles(httpContextAccessor),
                    GetUserId(httpContextAccessor)));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<PlaceDetailModel>>()
            .ProducesResponse<Guid>(200, "The created or updated place ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this place.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The place failed input validation.", "application/problem+json");

        placeModifyingEndpoints.MapDelete("{id:guid}", (Guid id, IPlaceFacade placeFacade,
            IHttpContextAccessor httpContextAccessor) =>
        {
            if (placeFacade.GetById(id) is null)
                return Results.NotFound();
            try
            {
                placeFacade.Delete(id, GetUserRoles(httpContextAccessor), GetUserId(httpContextAccessor));
                return Results.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        })
            .ProducesResponse(200, "The place was deleted.")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot delete this place.", "application/problem+json")
            .ProducesResponse(404, "The place does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The place is referenced by existing records and cannot be deleted.", "application/problem+json");

        return endpointRouteBuilder;
    }

    private static IResult Forbidden() =>
        Results.Problem(statusCode: StatusCodes.Status403Forbidden,
            title: PlaceEndpointsResources.Forbidden_Title, detail: PlaceEndpointsResources.Forbidden_Detail);
}
