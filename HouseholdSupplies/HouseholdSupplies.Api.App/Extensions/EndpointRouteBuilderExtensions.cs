using HouseholdSupplies.Api.App.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HouseholdSupplies.Api.App.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder UsePlaceEndpoints(
        this IEndpointRouteBuilder endpointRouteBuilder,
        IOptions<IdentityOptions> identityOptions)
        => new PlaceEndpoints(identityOptions)
            .MapEndpoints(endpointRouteBuilder);

    public static IEndpointRouteBuilder UseShoppingListEndpoints(
        this IEndpointRouteBuilder endpointRouteBuilder,
        IOptions<IdentityOptions> identityOptions)
        => new ShoppingListEndpoints(identityOptions)
            .MapEndpoints(endpointRouteBuilder);
    public static IEndpointRouteBuilder UseItemEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        => new ItemEndpoints().MapEndpoints(endpointRouteBuilder);

    public static IEndpointRouteBuilder UseUserEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        => new UserEndpoints().MapEndpoints(endpointRouteBuilder);
}