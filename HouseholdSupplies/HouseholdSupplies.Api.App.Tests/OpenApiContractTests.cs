using System.Text.Json;

namespace HouseholdSupplies.Api.App.Tests;

public sealed partial class EndpointTests
{
    [Theory]
    [InlineData("/api/item", "get", "ItemListModel[]", "200,400")]
    [InlineData("/api/item/{id}", "get", "ItemDetailModel", "200,404")]
    [InlineData("/api/item", "post", "Guid", "200,400,409,422")]
    [InlineData("/api/item", "put", "Guid", "200,400,403,404,409,422")]
    [InlineData("/api/item/save", "post", "Guid", "200,400,403,409,422")]
    [InlineData("/api/item/{id}", "delete", "Empty", "200,403,404,409")]
    [InlineData("/api/item/{id}/shopping-list/{shoppingListId}", "post", "Guid", "200,400,404,409")]
    [InlineData("/api/user", "get", "UserListModel[]", "200,400")]
    [InlineData("/api/user/{id}", "get", "UserListModel", "200,404")]
    [InlineData("/api/user", "post", "Guid", "200,400,409,422")]
    [InlineData("/api/user", "put", "Guid", "200,400,404,409,422")]
    [InlineData("/api/user/{id}", "delete", "Empty", "200,404,409")]
    [InlineData("/api/place", "get", "PlaceListModel[]", "200,400")]
    [InlineData("/api/place/{id}", "get", "PlaceDetailModel", "200,404")]
    [InlineData("/api/place", "post", "Guid", "200,400,409,422")]
    [InlineData("/api/place", "put", "Guid", "200,400,403,404,409,422")]
    [InlineData("/api/place/save", "post", "Guid", "200,400,403,409,422")]
    [InlineData("/api/place/{id}", "delete", "Empty", "200,403,404,409")]
    [InlineData("/api/shopping-list", "get", "ShoppingListListModel[]", "200,400")]
    [InlineData("/api/shopping-list/{id}", "get", "ShoppingListDetailModel", "200,404")]
    [InlineData("/api/shopping-list", "post", "Guid", "200,400,409,422")]
    [InlineData("/api/shopping-list", "put", "Guid", "200,400,403,404,409,422")]
    [InlineData("/api/shopping-list/save", "post", "Guid", "200,400,403,409,422")]
    [InlineData("/api/shopping-list/{id}", "delete", "Empty", "200,403,404,409")]
    public async Task OpenApi_DescribesActualResponses(string path, string method, string resultType, string statuses)
    {
        // Inspect the document clients actually download, rather than endpoint metadata alone.
        using var document = JsonDocument.Parse(await _client.GetStringAsync("/swagger/v1/swagger.json"));
        var root = document.RootElement;
        var responses = root.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses");
        Assert.Equal(statuses.Split(',').Order(), responses.EnumerateObject().Select(response => response.Name).Order());
        foreach (var response in responses.EnumerateObject())
            Assert.False(string.IsNullOrWhiteSpace(response.Value.GetProperty("description").GetString()),
                $"{method} {path}: response {response.Name} needs a description.");

        var success = responses.GetProperty("200");
        if (resultType == "Empty")
        {
            Assert.False(success.TryGetProperty("content", out var content) && content.EnumerateObject().Any());
        }
        else
        {
            var schema = success.GetProperty("content").GetProperty("application/json").GetProperty("schema");
            if (resultType == "Guid")
            {
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Contains(schema.GetProperty("format").GetString(), new[] { "uuid", "guid" });
            }
            else
            {
                if (resultType.EndsWith("[]"))
                {
                    Assert.Equal("array", schema.GetProperty("type").GetString());
                    schema = schema.GetProperty("items");
                }
                Assert.Equal($"#/components/schemas/{resultType.TrimEnd('[', ']')}", schema.GetProperty("$ref").GetString());
                Assert.True(HasProperty(root, schema, "id"));
                Assert.True(HasProperty(root, schema, "name"));
            }
        }

        foreach (var response in responses.EnumerateObject().Where(response => response.Name != "200"))
        {
            var status = response.Name;
            if (status is "403" or "409" or "422"
                || (status == "400" && path is "/api/item" or "/api/place" or "/api/shopping-list" && method is "post" or "put")
                || (status == "400" && path.EndsWith("/save")))
            {
                var schema = response.Value.GetProperty("content").GetProperty("application/problem+json").GetProperty("schema");
                Assert.True(HasProperty(root, schema, "status"));
                if (status == "422" || status == "400")
                    Assert.True(HasProperty(root, schema, "errors"));
            }
            else if (status == "400")
            {
                var schema = response.Value.GetProperty("content").GetProperty("application/json").GetProperty("schema");
                Assert.True(HasProperty(root, schema, "error"));
            }
        }
    }

    private static bool HasProperty(JsonElement root, JsonElement schema, string property)
    {
        if (schema.TryGetProperty("$ref", out var reference))
            return HasProperty(root, root.GetProperty("components").GetProperty("schemas")
                .GetProperty(reference.GetString()!.Split('/').Last()), property);
        if (schema.TryGetProperty("properties", out var properties) && properties.TryGetProperty(property, out _))
            return true;
        return schema.TryGetProperty("allOf", out var allOf) && allOf.EnumerateArray().Any(part => HasProperty(root, part, property));
    }
}
