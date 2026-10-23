using HouseholdSupplies.Api.App.Models;
using HouseholdSupplies.Api.App.Resources.Texts;
using Microsoft.AspNetCore.Http.Metadata;

namespace HouseholdSupplies.Api.App.Extensions;

public static class RequestErrorResponseExtensions
{
    public static IApplicationBuilder UseRequestErrorResponses(this IApplicationBuilder application)
        => application.UseStatusCodePages(async statusCodeContext =>
        {
            var context = statusCodeContext.HttpContext;
            if (context.Response.StatusCode != StatusCodes.Status400BadRequest)
                return;

            var message = EntityEndpointsResources.BadRequest_Detail;
            var responseMetadata = context.GetEndpoint()?.Metadata
                .GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .LastOrDefault(response => response.StatusCode == StatusCodes.Status400BadRequest);

            // Match the endpoint's declared error format; existing response bodies are left intact.
            IResult result = responseMetadata?.Type == typeof(HttpValidationProblemDetails)
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = [message] },
                    statusCode: StatusCodes.Status400BadRequest, title: EntityEndpointsResources.BadRequest_Title)
                : Results.BadRequest(new ApiErrorResponse(message));

            await result.ExecuteAsync(context);
        });
}
