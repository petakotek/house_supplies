namespace HouseholdSupplies.Api.App.Extensions;

public static class ResponseMetadataExtensions
{
    public static RouteHandlerBuilder ProducesResponse<T>(this RouteHandlerBuilder builder,
        int statusCode, string description, string contentType = "application/json")
        => builder.WithMetadata(new ProducesResponseTypeMetadata(statusCode, typeof(T), [contentType])
        {
            Description = description
        });

    public static RouteHandlerBuilder ProducesResponse(this RouteHandlerBuilder builder,
        int statusCode, string description)
        => builder.WithMetadata(new ProducesResponseTypeMetadata(statusCode, typeof(void), [])
        {
            Description = description
        });
}
