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

    [Theory]
    [InlineData("Production", "en", "invalid parameter values")]
    [InlineData("Development", "cs", "neplatné hodnoty parametrů")]
    public async Task InvalidQueryFormats_ReturnLocalizedMessages(string environment, string culture, string expectedMessage)
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        foreach (var route in new[] { "item?ascending=wrong", "item?placeId=wrong", "user?page=wrong", "user?ascending=wrong" })
        {
            var response = await client.GetAsync($"/api/{route}&culture={culture}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Contains(expectedMessage, body.GetProperty("error").GetString());
        }
    }

    [Theory]
    [InlineData("Production", "en", "invalid parameter values")]
    [InlineData("Development", "cs", "neplatné hodnoty parametrů")]
    public async Task InvalidBodies_ReturnLocalizedMessagesInDocumentedFormats(string environment, string culture, string expectedMessage)
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var routes = new[]
        {
            (HttpMethod.Post, "item", true),
            (HttpMethod.Put, "item", true),
            (HttpMethod.Post, "item/save", true),
            (HttpMethod.Post, "user", false),
            (HttpMethod.Put, "user", false),
            (HttpMethod.Post, $"item/{Guid.NewGuid()}/shopping-list/{_listId}", false)
        };

        foreach (var (method, route, validationProblem) in routes)
        foreach (var json in new[] { "{", "", "null" })
        {
            using var request = new HttpRequestMessage(method, $"/api/{route}?culture={culture}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(validationProblem ? "application/problem+json" : "application/json",
                response.Content.Headers.ContentType?.MediaType);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (validationProblem)
            {
                Assert.Equal(400, body.GetProperty("status").GetInt32());
                Assert.Equal(culture == "cs" ? "Neplatný požadavek" : "Invalid request", body.GetProperty("title").GetString());
                Assert.Contains(expectedMessage, body.GetProperty("errors").GetProperty("request")[0].GetString());
            }
            else
            {
                Assert.Contains(expectedMessage, body.GetProperty("error").GetString());
            }
        }
    }

    [Fact]
    public async Task RequestErrors_PreserveExistingValidationAndNotFoundResponses()
    {
        var paging = await _client.GetAsync("/api/user?page=0");
        Assert.Equal(HttpStatusCode.BadRequest, paging.StatusCode);
        var pagingBody = await paging.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Page and pageSize", pagingBody.GetProperty("error").GetString());

        var invalidReference = await _client.PostAsJsonAsync("/api/item", Item("Milk") with
        {
            Place = new PlaceListModel { Id = Guid.NewGuid(), Name = "Missing", ImageUrl = "image" }
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidReference.StatusCode);
        var referenceBody = await invalidReference.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(referenceBody.GetProperty("errors").TryGetProperty("Place.Id", out _));
        Assert.False(referenceBody.GetProperty("errors").TryGetProperty("request", out _));

        var invalidItem = await _client.PostAsJsonAsync("/api/item", Item(""));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidItem.StatusCode);
        var validationBody = await invalidItem.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(validationBody.GetProperty("errors").TryGetProperty("Name", out _));

        var missing = await _client.GetAsync($"/api/user/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("", await missing.Content.ReadAsStringAsync());
    }
}
