using System.Net;
using System.Net.Http.Json;
using HouseholdSupplies.Api.DAL;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.User;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.App.Tests;

public sealed partial class EndpointTests
{
    [Fact]
    public async Task UserUpdate_PreservesOwnershipAndAuthoredRecords()
    {
        // Arrange
        var create = await _client.PostAsJsonAsync("/api/item", Item("Milk"));
        create.EnsureSuccessStatusCode();
        var itemId = await create.Content.ReadFromJsonAsync<Guid>();
        const string ownerId = "external-user-id";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            db.Users.Single(user => user.Id == _authorId).OwnerId = ownerId;
            db.SaveChanges();
        }

        var update = new UserListModel
        {
            Id = _authorId, Name = "Updated author", ImageUrl = "https://example.com/updated.png", Role = Role.Admin
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/user", update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(update, await _client.GetFromJsonAsync<UserListModel>($"/api/user/{_authorId}"));
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HouseholdSuppliesDbContext>();
            Assert.Equal(ownerId, db.Users.Single(user => user.Id == _authorId).OwnerId);
            Assert.Equal(_authorId, db.Items.Single(item => item.Id == itemId).AuthorId);
            Assert.Equal(_authorId, db.Places.Single(place => place.Id == _placeId).AuthorId);
            Assert.Equal(_authorId, db.ShoppingLists.Single(list => list.Id == _listId).AuthorId);
        }

        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/user/{_authorId}")).StatusCode);
    }
}
