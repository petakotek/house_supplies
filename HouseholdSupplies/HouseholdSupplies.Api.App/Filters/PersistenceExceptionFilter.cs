using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using HouseholdSupplies.Api.App.Resources.Texts;

namespace HouseholdSupplies.Api.App.Filters;

public class PersistenceExceptionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                title: EntityEndpointsResources.Conflict_Title, detail: EntityEndpointsResources.Conflict_Detail);
        }
    }
}
