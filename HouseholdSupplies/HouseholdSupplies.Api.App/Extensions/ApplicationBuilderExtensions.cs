using HouseholdSupplies.Api.App.Middlewares;

namespace HouseholdSupplies.Api.App.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestCulture(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestCultureMiddleware>();
    }
}