using HouseholdSupplies.Api.BL.Facades;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;
using Moq;

namespace HouseholdSupplies.Api.BL.Tests;

public class ItemFacadeTests
{
    private readonly Mock<IItemRepository> repositoryMock = new(MockBehavior.Strict);
    private readonly ItemFacade facade;

    public ItemFacadeTests()
    {
        facade = new ItemFacade(repositoryMock.Object, new ItemMapper());
    }

    [Fact]
    public void Delete_Calls_Correct_Method_On_Repository()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        repositoryMock.Setup(repository => repository.Remove(itemId));

        // Act
        facade.Delete(itemId, []);

        // Assert
        repositoryMock.Verify(repository => repository.Remove(itemId), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Returns_Mapped_Detail()
    {
        // Arrange
        var entity = CreateEntity();
        repositoryMock.Setup(repository => repository.GetById(entity.Id)).Returns(entity);

        // Act
        var result = facade.GetById(entity.Id);

        // Assert
        Assert.NotNull(result);
        AssertDetailFields(entity, result);
        repositoryMock.Verify(repository => repository.GetById(entity.Id), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Returns_Null_For_Missing_Item()
    {
        var id = Guid.NewGuid();
        repositoryMock.Setup(repository => repository.GetById(id)).Returns((ItemEntity?)null);

        var result = facade.GetById(id);

        Assert.Null(result);
        repositoryMock.Verify(repository => repository.GetById(id), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Maps_Null_Optional_Fields()
    {
        var entity = CreateEntity() with
        {
            Description = null, ExpirationDate = null, Author = null, Place = null
        };
        repositoryMock.Setup(repository => repository.GetById(entity.Id)).Returns(entity);

        var result = facade.GetById(entity.Id);

        Assert.NotNull(result);
        AssertDetailFields(entity, result);
    }

    [Fact]
    public void Create_Inserts_Mapped_Entity_And_Returns_Repository_Id()
    {
        var model = CreateDetailModel();
        var insertedId = Guid.NewGuid();
        ItemEntity? inserted = null;
        repositoryMock.Setup(repository => repository.Insert(It.IsAny<ItemEntity>()))
            .Callback<ItemEntity>(entity => inserted = entity)
            .Returns(insertedId);

        var result = facade.Create(model, []);

        Assert.Equal(insertedId, result);
        Assert.NotNull(inserted);
        AssertDetailFields(inserted, model);
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ItemEntity>()), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Update_Passes_Mapped_Entity_And_Returns_Repository_Id()
    {
        var model = CreateDetailModel();
        var updatedId = Guid.NewGuid();
        ItemEntity? updated = null;
        repositoryMock.Setup(repository => repository.Update(It.IsAny<ItemEntity>()))
            .Callback<ItemEntity>(entity => updated = entity)
            .Returns(updatedId);

        var result = facade.Update(model, []);

        Assert.Equal(updatedId, result);
        Assert.NotNull(updated);
        AssertDetailFields(updated, model);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ItemEntity>()), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_Item()
    {
        var model = CreateDetailModel();
        repositoryMock.Setup(repository => repository.Update(It.Is<ItemEntity>(item => item.Id == model.Id)))
            .Returns((Guid?)null);

        var result = facade.Update(model, []);

        Assert.Null(result);
        repositoryMock.Verify(repository => repository.Update(It.Is<ItemEntity>(item => item.Id == model.Id)), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateOrUpdate_Selects_Correct_Repository_Operation(bool exists)
    {
        var model = CreateDetailModel();
        var savedId = Guid.NewGuid();
        ItemEntity? saved = null;
        repositoryMock.Setup(repository => repository.Exists(model.Id)).Returns(exists);
        if (exists)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<ItemEntity>()))
                .Callback<ItemEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<ItemEntity>()))
                .Callback<ItemEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = facade.CreateOrUpdate(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertDetailFields(saved, model);
        repositoryMock.Verify(repository => repository.Exists(model.Id), Times.Once);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ItemEntity>()), exists ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ItemEntity>()), exists ? Times.Never() : Times.Once());
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetAll_Returns_Mapped_List_Models()
    {
        var entity = CreateEntity();
        SetupItems(entity);

        var result = Assert.Single(facade.GetAll());

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.ImageUrl, result.ImageUrl);
        Assert.Equal(entity.Amount, result.Amount);
        Assert.Equal(entity.Category, result.Category);
        Assert.Equal(entity.Place?.Id, result.Place?.Id);
        Assert.Equal(entity.Place?.Name, result.Place?.Name);
        repositoryMock.Verify(repository => repository.GetAll(), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_When_Repository_Is_Empty()
    {
        SetupItems();

        Assert.Empty(facade.GetAll());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetAll_Without_Search_Returns_Items_With_Null_Optional_Fields(string? searchQuery)
    {
        var entity = CreateEntity() with { Description = null, Author = null, Place = null };
        SetupItems(entity);

        var result = Assert.Single(facade.GetAll(searchQuery: searchQuery));

        Assert.Equal(entity.Id, result.Id);
        Assert.Null(result.Place);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("Author")]
    [InlineData("Place")]
    public void GetAll_Searches_Name_Description_Author_And_Place(string field)
    {
        var matching = CreateEntity();
        switch (field)
        {
            case "Name": matching.Name = "Prefix NEEDLE suffix"; break;
            case "Description": matching.Description = "Prefix needle suffix"; break;
            case "Author": matching.Author!.Name = "Prefix needle suffix"; break;
            case "Place": matching.Place!.Name = "Prefix needle suffix"; break;
        }
        SetupItems(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(searchQuery: "needle"));

        Assert.Equal(matching.Id, result.Id);
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_When_Search_Does_Not_Match()
    {
        SetupItems(CreateEntity());

        Assert.Empty(facade.GetAll(searchQuery: "missing"));
    }

    [Theory]
    [InlineData("banana", "banana", "BANANA", "Banana")]
    [InlineData("BANANA", "banana", "BANANA", "Banana")]
    [InlineData("Banana", "banana", "BANANA", "Banana")]
    [InlineData("šunka", "šunka", "ŠUNKA", "Šunka")]
    [InlineData("ŠUNKA", "šunka", "ŠUNKA", "Šunka")]
    [InlineData("Šunka", "šunka", "ŠUNKA", "Šunka")]
    public void GetAll_Search_Ignores_Case_In_All_Searchable_Fields(
        string searchQuery, string lower, string upper, string mixed)
    {
        var lowerCase = CreateEntity(lower);
        var upperCase = CreateEntity(upper);
        var mixedCase = CreateEntity(mixed);
        var description = CreateEntity() with { Description = $"Fresh {upper} supplies" };
        var author = CreateEntity();
        author.Author!.Name = $"{mixed} supplier";
        var place = CreateEntity();
        place.Place!.Name = $"{upper} shelf";
        ItemEntity[] matches = [lowerCase, upperCase, mixedCase, description, author, place];
        SetupItems([CreateEntity("Apple"), .. matches]);

        var results = facade.GetAll(searchQuery: searchQuery).ToList();

        Assert.Equal(matches.Select(item => item.Id).Order(), results.Select(item => item.Id).Order());
    }

    [Theory]
    [InlineData("banana")]
    [InlineData("BANANA")]
    public void GetAll_Search_Matches_Name_When_Optional_Fields_Are_Null(string searchQuery)
    {
        var matching = CreateEntity("Banana") with { Description = null, Author = null, Place = null };
        var other = CreateEntity("Apple") with { Description = null, Author = null, Place = null };
        SetupItems(other, matching);

        var result = Assert.Single(facade.GetAll(searchQuery: searchQuery));

        Assert.Equal(matching.Id, result.Id);
    }

    [Fact]
    public void GetAll_Filters_By_Place_Id()
    {
        var matching = CreateEntity();
        SetupItems(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(placeId: matching.PlaceId));

        Assert.Equal(matching.Id, result.Id);
    }

    [Theory]
    [InlineData(Category.Food)]
    [InlineData(Category.CleaningSupplies)]
    [InlineData(Category.PersonalCare)]
    public void GetAll_Filters_By_Numeric_Category(Category category)
    {
        var entities = Enum.GetValues<Category>()
            .Select(value => CreateEntity() with { Category = value }).ToArray();
        SetupItems(entities);

        var result = Assert.Single(facade.GetAll(categoryName: ((int)category).ToString()));

        Assert.Equal(entities.Single(item => item.Category == category).Id, result.Id);
        Assert.Equal(category, result.Category);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAll_Orders_By_Name(bool ascending)
    {
        var apple = CreateEntity("Apple");
        var banana = CreateEntity("Banana");
        var carrot = CreateEntity("Carrot");
        SetupItems(banana, carrot, apple);

        var results = facade.GetAll(ascending: ascending).ToList();

        Guid[] expected = ascending ? [apple.Id, banana.Id, carrot.Id] : [carrot.Id, banana.Id, apple.Id];
        Assert.Equal(expected, results.Select(item => item.Id));
    }

    [Theory]
    [InlineData(1, 2, new[] { "Apple", "Banana" })]
    [InlineData(2, 2, new[] { "Carrot", "Dates" })]
    [InlineData(3, 2, new[] { "Eggs" })]
    [InlineData(4, 2, new string[0])]
    public void GetAll_Returns_Requested_Page(int page, int pageSize, string[] expectedNames)
    {
        SetupItems(CreateEntity("Eggs"), CreateEntity("Carrot"), CreateEntity("Apple"),
            CreateEntity("Dates"), CreateEntity("Banana"));

        var results = facade.GetAll(page: page, pageSize: pageSize).ToList();

        Assert.Equal(expectedNames, results.Select(item => item.Name));
    }

    [Fact]
    public void GetAll_Default_Page_Contains_First_25_Items_In_Ascending_Order()
    {
        var entities = Enumerable.Range(1, 30).Select(index => CreateEntity($"Item {index:D2}")).ToArray();
        SetupItems(entities.Reverse().ToArray());

        var results = facade.GetAll().ToList();

        Assert.Equal(entities.Take(25).Select(item => item.Id), results.Select(item => item.Id));
    }

    [Fact]
    public void GetAll_Applies_All_Filters_And_Descending_Order_Before_Paging()
    {
        var first = CreateEntity("Apple match");
        var second = CreateEntity("Banana match") with { PlaceId = first.PlaceId, Place = first.Place };
        var third = CreateEntity("Carrot match") with { PlaceId = first.PlaceId, Place = first.Place };
        var wrongPlace = CreateEntity("Dates match");
        var wrongCategory = first with { Id = Guid.NewGuid(), Name = "Eggs match", Category = Category.PersonalCare };
        var wrongSearch = first with { Id = Guid.NewGuid(), Name = "Flour" };
        SetupItems(wrongSearch, first, wrongPlace, second, wrongCategory, third);

        var result = Assert.Single(facade.GetAll(searchQuery: "match", ascending: false,
            placeId: first.PlaceId, categoryName: ((int)Category.Food).ToString(), page: 2, pageSize: 1));

        Assert.Equal(second.Id, result.Id);
    }

    private void SetupItems(params ItemEntity[] entities)
    {
        repositoryMock.Setup(repository => repository.GetAll()).Returns(entities.AsQueryable());
    }

    private static ItemEntity CreateEntity(string name = "Milk")
    {
        var author = new UserEntity
        {
            Id = Guid.NewGuid(), Name = "Alice", ImageUrl = "alice.png", Role = Role.User
        };
        var place = new PlaceEntity
        {
            Id = Guid.NewGuid(), Name = "Pantry", AuthorId = author.Id
        };
        return new ItemEntity
        {
            Id = Guid.NewGuid(), Name = name, ImageUrl = "milk.png", Amount = 2.5,
            Category = Category.Food, Description = "Fresh supplies",
            ExpirationDate = new DateTime(2027, 1, 15),
            AuthorId = author.Id, Author = author, PlaceId = place.Id, Place = place
        };
    }

    private static ItemDetailModel CreateDetailModel()
    {
        var entity = CreateEntity();
        return new ItemDetailModel
        {
            Id = entity.Id, Name = entity.Name, ImageUrl = entity.ImageUrl, Amount = entity.Amount,
            Category = entity.Category, Description = entity.Description, ExpirationDate = entity.ExpirationDate,
            AuthorId = entity.AuthorId, Author = entity.Author, PlaceId = entity.PlaceId, Place = entity.Place
        };
    }

    private static void AssertDetailFields(ItemEntity entity, ItemDetailModel model)
    {
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Name, model.Name);
        Assert.Equal(entity.ImageUrl, model.ImageUrl);
        Assert.Equal(entity.Amount, model.Amount);
        Assert.Equal(entity.Category, model.Category);
        Assert.Equal(entity.Description, model.Description);
        Assert.Equal(entity.ExpirationDate, model.ExpirationDate);
        Assert.Equal(entity.AuthorId, model.AuthorId);
        Assert.Equal(entity.Author?.Id, model.Author?.Id);
        Assert.Equal(entity.Author?.Name, model.Author?.Name);
        Assert.Equal(entity.PlaceId, model.PlaceId);
        Assert.Equal(entity.Place?.Id, model.Place?.Id);
        Assert.Equal(entity.Place?.Name, model.Place?.Name);
    }
}
