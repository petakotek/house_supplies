using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Place;
using HouseholdSupplies.Common.Models.ShoppingList;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Api.App.Tests;

public sealed partial class EndpointTests
{
    [Fact]
    public async Task PlaceCrud_Save_PersistChanges_AndReturnNotFoundAfterDeletion()
    {
        var model = Place("Cellar") with { Id = Guid.NewGuid() };
        var create = await _client.PostAsJsonAsync("/api/place", model);
        create.EnsureSuccessStatusCode();
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(model.Id, id);
        var update = model with { Id = id, Name = "Basement" };
        (await _client.PutAsJsonAsync("/api/place", update)).EnsureSuccessStatusCode();
        Assert.Equal("Basement", (await _client.GetFromJsonAsync<PlaceDetailModel>($"/api/place/{id}"))!.Name);
        (await _client.PostAsJsonAsync("/api/place/save", update with { Name = "Garage" })).EnsureSuccessStatusCode();
        var detail = await _client.GetFromJsonAsync<PlaceDetailModel>($"/api/place/{id}");
        Assert.Equal("Garage", detail!.Name);
        Assert.Equal(_authorId, detail.Author!.Id);
        (await _client.DeleteAsync($"/api/place/{id}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/place/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/place/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/place", update)).StatusCode);
        var save = await _client.PostAsJsonAsync("/api/place/save", update with { Id = Guid.Empty });
        save.EnsureSuccessStatusCode();
        Assert.NotEqual(Guid.Empty, await save.Content.ReadFromJsonAsync<Guid>());
    }

    [Fact]
    public async Task ShoppingListCrud_Save_PersistChanges_AndReturnNotFoundAfterDeletion()
    {
        var model = ShoppingList("Weekend") with { Id = Guid.NewGuid() };
        var create = await _client.PostAsJsonAsync("/api/shopping-list", model);
        create.EnsureSuccessStatusCode();
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(model.Id, id);
        var update = model with { Id = id, Shop = "Market" };
        (await _client.PutAsJsonAsync("/api/shopping-list", update)).EnsureSuccessStatusCode();
        Assert.Equal("Market", (await _client.GetFromJsonAsync<ShoppingListDetailModel>($"/api/shopping-list/{id}"))!.Shop);
        (await _client.PostAsJsonAsync("/api/shopping-list/save", update with { Name = "Party" })).EnsureSuccessStatusCode();
        var detail = await _client.GetFromJsonAsync<ShoppingListDetailModel>($"/api/shopping-list/{id}");
        Assert.Equal("Party", detail!.Name);
        Assert.Equal(_authorId, detail.Author!.Id);
        (await _client.DeleteAsync($"/api/shopping-list/{id}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/shopping-list/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/shopping-list/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/shopping-list", update)).StatusCode);
        var save = await _client.PostAsJsonAsync("/api/shopping-list/save", update with { Id = Guid.Empty });
        save.EnsureSuccessStatusCode();
        Assert.NotEqual(Guid.Empty, await save.Content.ReadFromJsonAsync<Guid>());
    }

    [Fact]
    public async Task PlaceList_SearchesDescription_FiltersSortsAndPagesInSqlServer()
    {
        foreach (var name in new[] { "Attic", "Basement", "Cellar" })
            (await _client.PostAsJsonAsync("/api/place", Place(name) with { Description = "Shared description" })).EnsureSuccessStatusCode();
        var places = await _client.GetFromJsonAsync<PlaceListModel[]>(
            $"/api/place?searchQuery=DESCRIPTION&authorId={_authorId}&ascending=false&page=2&pageSize=1");
        Assert.Equal("Basement", Assert.Single(places!).Name);
        Assert.Empty((await _client.GetFromJsonAsync<PlaceListModel[]>($"/api/place?authorId={Guid.NewGuid()}"))!);
    }

    [Fact]
    public async Task ShoppingListList_SearchesShop_FiltersSortsAndPagesInSqlServer()
    {
        foreach (var name in new[] { "Breakfast", "Dinner", "Lunch" })
            (await _client.PostAsJsonAsync("/api/shopping-list", ShoppingList(name) with { Shop = "Corner Market" })).EnsureSuccessStatusCode();
        var lists = await _client.GetFromJsonAsync<ShoppingListListModel[]>(
            $"/api/shopping-list?searchQuery=MARKET&authorId={_authorId}&ascending=false&page=2&pageSize=1");
        Assert.Equal("Dinner", Assert.Single(lists!).Name);
        Assert.Empty((await _client.GetFromJsonAsync<ShoppingListListModel[]>($"/api/shopping-list?authorId={Guid.NewGuid()}"))!);
    }

    [Theory]
    [InlineData("place?page=0")]
    [InlineData("place?pageSize=-1")]
    [InlineData("place?page=2147483647&pageSize=2147483647")]
    [InlineData("shopping-list?page=0")]
    [InlineData("shopping-list?pageSize=-1")]
    [InlineData("shopping-list?page=2147483647&pageSize=2147483647")]
    public async Task InvalidPlaceAndShoppingListParameters_ReturnBadRequest(string route)
    {
        var response = await _client.GetAsync($"/api/{route}&culture=cs");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Page a pageSize musí být kladné a v rozsahu integeru.", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task PlaceAndShoppingListValidation_ReturnClientErrors()
    {
        var missingAuthor = new UserListModel { Id = Guid.NewGuid(), Name = "Missing", ImageUrl = "image", Role = Role.User };
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync("/api/place", Place("Cellar") with { Author = missingAuthor })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync("/api/shopping-list", ShoppingList("Weekend") with { Author = missingAuthor })).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.PostAsJsonAsync("/api/place", Place(""))).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.PostAsJsonAsync("/api/shopping-list", ShoppingList("") with { Shop = "" })).StatusCode);
    }

    [Fact]
    public async Task PlaceDelete_WithStoredItems_ReturnsConflict()
    {
        (await _client.PostAsJsonAsync("/api/item", Item("Milk"))).EnsureSuccessStatusCode();

        var conflict = await _client.DeleteAsync($"/api/place/{_placeId}");

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/place/{_placeId}")).StatusCode);
    }

    [Fact]
    public async Task ShoppingListDelete_WithEntries_DeletesEntriesAndKeepsItems()
    {
        var create = await _client.PostAsJsonAsync("/api/item", Item("Milk"));
        create.EnsureSuccessStatusCode();
        var itemId = await create.Content.ReadFromJsonAsync<Guid>();
        (await _client.PostAsJsonAsync($"/api/item/{itemId}/shopping-list/{_listId}", new { amount = 2 })).EnsureSuccessStatusCode();
        var list = await _client.GetFromJsonAsync<ShoppingListDetailModel>($"/api/shopping-list/{_listId}");
        Assert.Equal(_placeId, Assert.Single(list!.Items).Item!.Place!.Id);

        (await _client.DeleteAsync($"/api/shopping-list/{_listId}")).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/shopping-list/{_listId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/item/{itemId}")).StatusCode);
    }

    private PlaceDetailModel Place(string name) => new()
    {
        Name = name, ImageUrl = "https://example.com/place.png",
        Author = new UserListModel { Id = _authorId, Name = "Author", ImageUrl = "image", Role = Role.User }
    };

    private ShoppingListDetailModel ShoppingList(string name) => new()
    {
        Name = name, Shop = "Shop",
        Author = new UserListModel { Id = _authorId, Name = "Author", ImageUrl = "image", Role = Role.User }
    };
}
