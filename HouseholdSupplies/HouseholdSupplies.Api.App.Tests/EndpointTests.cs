using System.Net;
using System.Net.Http.Json;
using HouseholdSupplies.Api.DAL;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;
using HouseholdSupplies.Common.Models.Place;
using HouseholdSupplies.Common.Models.User;
using HouseholdSupplies.Api.DAL.TestInfrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.TestHost;

namespace HouseholdSupplies.Api.App.Tests;

[Trait("Category", "Integration")]
public sealed partial class EndpointTests : IClassFixture<SqlServerTestDatabase>, IDisposable
{
    private readonly SqlServerTestDatabase _database;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _placeId = Guid.NewGuid();
    private readonly Guid _listId = Guid.NewGuid();

    public EndpointTests(SqlServerTestDatabase database)
    {
        _database = database;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<HouseholdSuppliesDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<HouseholdSuppliesDbContext>>();
                services.AddDbContext<HouseholdSuppliesDbContext>(options =>
                    options.UseAzureSql(_database.ConnectionString));
            }));
        HttpClient? client = null;
        try
        {
            client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            _client = client;
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            db.Users.Add(new UserEntity { Id = _authorId, Name = "Author", ImageUrl = "https://example.com/user.png", Role = Role.User });
            db.Places.Add(new PlaceEntity { Id = _placeId, Name = "Pantry", ImageUrl = "https://example.com/place.png", AuthorId = _authorId });
            db.ShoppingLists.Add(new ShoppingListEntity { Id = _listId, Name = "Shopping", Shop = "Shop", AuthorId = _authorId });
            db.SaveChanges();
        }
        catch
        {
            client?.Dispose();
            _factory.Dispose();
            _database.Reset();
            throw;
        }
    }

    [Fact]
    public async Task UserCrud_PersistsChanges_AndReturnsNotFoundAfterDeletion()
    {
        var model = new UserListModel { Name = "New user", ImageUrl = "https://example.com/user.png", Role = Role.User };
        var create = await _client.PostAsJsonAsync("/api/user", model);
        create.EnsureSuccessStatusCode();
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        var update = model with { Id = id, Name = "Updated user", Role = Role.Admin };
        (await _client.PutAsJsonAsync("/api/user", update)).EnsureSuccessStatusCode();
        var detail = await _client.GetFromJsonAsync<UserListModel>($"/api/user/{id}");
        Assert.Equal(update, detail);
        (await _client.DeleteAsync($"/api/user/{id}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/user/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/user/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/user", update)).StatusCode);
    }

    [Fact]
    public async Task ItemCrud_Save_AndShoppingListEntry_PersistCorrectly()
    {
        var model = Item("Milk");
        var create = await _client.PostAsJsonAsync("/api/item", model);
        create.EnsureSuccessStatusCode();
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        var update = model with { Id = id, Amount = 5 };
        (await _client.PutAsJsonAsync("/api/item", update)).EnsureSuccessStatusCode();
        var detail = await _client.GetFromJsonAsync<ItemDetailModel>($"/api/item/{id}");
        Assert.Equal(5, detail!.Amount);
        Assert.Equal(_authorId, detail.Author!.Id);
        Assert.Equal(_placeId, detail.Place!.Id);
        (await _client.PostAsJsonAsync("/api/item/save", update with { Amount = 7 })).EnsureSuccessStatusCode();
        Assert.Equal(7, (await _client.GetFromJsonAsync<ItemDetailModel>($"/api/item/{id}"))!.Amount);
        var entry = await _client.PostAsJsonAsync($"/api/item/{id}/shopping-list/{_listId}", new { amount = 2 });
        entry.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            Assert.True(db.ShoppingListItems.Any(x => x.ItemId == id && x.ShoppingListId == _listId && x.Amount == 2));
        }
        (await _client.DeleteAsync($"/api/item/{id}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/item/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/item/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/item", update)).StatusCode);
        var save = await _client.PostAsJsonAsync("/api/item/save", update);
        save.EnsureSuccessStatusCode();
        Assert.Equal(id, await save.Content.ReadFromJsonAsync<Guid>());
    }

    [Fact]
    public async Task ItemList_SearchesDescription_FiltersSortsAndPagesInSqlServer()
    {
        foreach (var name in new[] { "Apple", "Banana", "Carrot" })
            (await _client.PostAsJsonAsync("/api/item", Item(name) with { Description = "Shared description" })).EnsureSuccessStatusCode();
        var items = await _client.GetFromJsonAsync<ItemListModel[]>(
            $"/api/item?searchQuery=DESCRIPTION&placeId={_placeId}&categoryName=Food&ascending=false&page=2&pageSize=1");
        Assert.Equal("Banana", Assert.Single(items!).Name);
        Assert.Equal(_placeId, items![0].Place!.Id);
        Assert.Empty((await _client.GetFromJsonAsync<ItemListModel[]>($"/api/item?placeId={Guid.NewGuid()}"))!);
        Assert.Empty((await _client.GetFromJsonAsync<ItemListModel[]>("/api/item?categoryName=CleaningSupplies"))!);
    }

    [Fact]
    public async Task UserList_SearchesFiltersSortsAndPages()
    {
        foreach (var name in new[] { "Anna", "Annie", "Anne" })
            (await _client.PostAsJsonAsync("/api/user", new UserListModel { Name = name, ImageUrl = "image", Role = Role.Admin })).EnsureSuccessStatusCode();
        var users = await _client.GetFromJsonAsync<UserListModel[]>("/api/user?searchQuery=ANN&role=Admin&ascending=false&page=2&pageSize=1");
        Assert.Equal("Anne", Assert.Single(users!).Name);
    }

    [Theory]
    [InlineData("item?page=0")]
    [InlineData("item?pageSize=-1")]
    [InlineData("item?page=2147483647&pageSize=2147483647")]
    [InlineData("item?categoryName=Unknown")]
    [InlineData("user?page=0")]
    [InlineData("user?role=999")]
    public async Task InvalidListParameters_ReturnBadRequest(string route)
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync($"/api/{route}")).StatusCode);
    }

    [Fact]
    public async Task ValidationAndReferences_ReturnClientErrors()
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.PostAsJsonAsync("/api/item", Item("") with { Amount = -1 })).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.PostAsJsonAsync("/api/user", new UserListModel { Name = "", ImageUrl = "image", Role = (Role)99 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync("/api/item", Item("Bad reference") with { Place = new PlaceListModel { Id = Guid.NewGuid(), Name = "Missing", ImageUrl = "image" } })).StatusCode);
        var conflict = await _client.DeleteAsync($"/api/user/{_authorId}?culture=cs");
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Contains("Konfliktní vazby", System.Text.Json.JsonDocument.Parse(await conflict.Content.ReadAsStringAsync())
            .RootElement.GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/item/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task ShoppingListEntry_RejectsInvalidAmountAndMissingRecords()
    {
        var create = await _client.PostAsJsonAsync("/api/item", Item("Milk"));
        create.EnsureSuccessStatusCode();
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync($"/api/item/{id}/shopping-list/{_listId}", new { amount = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsJsonAsync($"/api/item/{id}/shopping-list/{Guid.NewGuid()}", new { amount = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.PostAsJsonAsync($"/api/item/{Guid.NewGuid()}/shopping-list/{_listId}", new { amount = 1 })).StatusCode);
    }

    private ItemDetailModel Item(string name) => new()
    {
        Name = name, ImageUrl = "https://example.com/item.png", Amount = 1, Category = Category.Food,
        Author = new UserListModel { Id = _authorId, Name = "Author", ImageUrl = "image", Role = Role.User },
        Place = new PlaceListModel { Id = _placeId, Name = "Pantry", ImageUrl = "image" }
    };

    public void Dispose()
    {
        try
        {
            _client.Dispose();
            _factory.Dispose();
        }
        finally
        {
            _database.Reset();
        }
    }
}
