using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HouseholdSupplies.Common.Models.Place;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HouseholdSupplies.Api.App.Tests;

public sealed partial class EndpointTests
{
    [Theory]
    [InlineData("/api/user?role=Unknown&culture=cs", "Role musí být platný název nebo číslo role.")]
    [InlineData("/api/item?categoryName=Unknown&culture=cs", "Kategorie musí být platný název nebo číslo kategorie.")]
    public async Task InvalidFilters_UseEndpointSpecificCzechResources(string route, string expectedMessage)
    {
        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedMessage, body.GetProperty("error").GetString());
    }

}
