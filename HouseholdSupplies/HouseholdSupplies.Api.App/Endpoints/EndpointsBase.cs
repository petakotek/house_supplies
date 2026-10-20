using System.Security.Claims;

namespace HouseholdSupplies.Api.App.Endpoints;

public abstract class EndpointsBase
{
    public abstract IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder);
    
    
    public string? GetUserId(IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public IList<string> GetUserRoles(IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.User.Claims
               .Where(c => c.Type == ClaimTypes.Role)
               .Select(c => c.Value)
               .ToList()
           ?? [];
}