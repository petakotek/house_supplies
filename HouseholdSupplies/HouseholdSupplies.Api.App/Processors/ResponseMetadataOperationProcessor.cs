using System.Globalization;
using Microsoft.AspNetCore.Http.Metadata;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace HouseholdSupplies.Api.App.Processors;

public class ResponseMetadataOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // NSwag 14.7 generates schemas, but drops minimal API response descriptions
        // and collapses per-status content types into application/json.
        if (context is AspNetCoreOperationProcessorContext aspNetContext)
        {
            var metadata = aspNetContext.ApiDescription.ActionDescriptor.EndpointMetadata
                .OfType<IProducesResponseTypeMetadata>();
            foreach (var response in metadata.Where(response => !string.IsNullOrWhiteSpace(response.Description)))
            {
                if (context.OperationDescription.Operation.Responses.TryGetValue(
                        response.StatusCode.ToString(CultureInfo.InvariantCulture), out var documentedResponse))
                {
                    documentedResponse.Description = response.Description;
                    var content = documentedResponse.Content.Values.FirstOrDefault();
                    if (content is not null && response.ContentTypes.Any())
                    {
                        documentedResponse.Content.Clear();
                        foreach (var contentType in response.ContentTypes)
                            documentedResponse.Content[contentType] = content;
                    }
                }
            }
        }

        return true;
    }
}
