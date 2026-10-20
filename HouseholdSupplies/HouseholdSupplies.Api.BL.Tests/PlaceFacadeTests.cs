using HouseholdSupplies.Api.BL.Facades;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.Place;
using Moq;

namespace HouseholdSupplies.Api.BL.Tests;

public class PlaceFacadeTests
{
    private readonly Mock<IPlaceRepository> repositoryMock = new(MockBehavior.Strict);
    private readonly PlaceFacade facade;

    public PlaceFacadeTests()
    {
        facade = new PlaceFacade(repositoryMock.Object, new PlaceMapper());
    }

    [Fact]
    public void Delete_Calls_Repository_With_Requested_Id()
    {
        var id = Guid.NewGuid();
        repositoryMock.Setup(repository => repository.Remove(id));

        facade.Delete(id, []);

        repositoryMock.Verify(repository => repository.Remove(id), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Returns_Mapped_Detail()
    {
        var entity = CreateEntity();
        repositoryMock.Setup(repository => repository.GetById(entity.Id)).Returns(entity);

        var result = facade.GetById(entity.Id);

        Assert.NotNull(result);
        AssertDetailFields(entity, result);
        repositoryMock.Verify(repository => repository.GetById(entity.Id), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Returns_Null_For_Missing_Entity()
    {
        var id = Guid.NewGuid();
        repositoryMock.Setup(repository => repository.GetById(id)).Returns((PlaceEntity?)null);

        Assert.Null(facade.GetById(id));

        repositoryMock.Verify(repository => repository.GetById(id), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_Passes_Mapped_Entity_And_Returns_Repository_Id(bool update)
    {
        var model = CreateDetailModel();
        var savedId = Guid.NewGuid();
        PlaceEntity? saved = null;
        if (update)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<PlaceEntity>()))
                .Callback<PlaceEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<PlaceEntity>()))
                .Callback<PlaceEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = update ? facade.Update(model, []) : facade.Create(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertSavedEntity(model, saved);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<PlaceEntity>()), update ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<PlaceEntity>()), update ? Times.Never() : Times.Once());
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_Entity()
    {
        var model = CreateDetailModel();
        repositoryMock.Setup(repository => repository.Update(It.Is<PlaceEntity>(entity => entity.Id == model.Id)))
            .Returns((Guid?)null);

        Assert.Null(facade.Update(model, []));

        repositoryMock.Verify(repository => repository.Update(It.Is<PlaceEntity>(entity => entity.Id == model.Id)), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateOrUpdate_Selects_Correct_Repository_Operation(bool exists)
    {
        var model = CreateDetailModel();
        var savedId = Guid.NewGuid();
        PlaceEntity? saved = null;
        repositoryMock.Setup(repository => repository.Exists(model.Id)).Returns(exists);
        if (exists)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<PlaceEntity>()))
                .Callback<PlaceEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<PlaceEntity>()))
                .Callback<PlaceEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = facade.CreateOrUpdate(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertSavedEntity(model, saved);
        repositoryMock.Verify(repository => repository.Exists(model.Id), Times.Once);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<PlaceEntity>()), exists ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<PlaceEntity>()), exists ? Times.Never() : Times.Once());
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_When_Repository_Is_Empty()
    {
        SetupEntities();

        Assert.Empty(facade.GetAll());

        repositoryMock.Verify(repository => repository.GetAll(), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    private void SetupEntities(params PlaceEntity[] entities)
    {
        repositoryMock.Setup(repository => repository.GetAll()).Returns(entities.AsQueryable());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetAll_Without_Search_Returns_All_Entities(string? searchQuery)
    {
        var first = CreateEntity("Alpha");
        var second = CreateEntity("Beta") with { Description = null, Author = null };
        SetupEntities(second, first);

        var results = facade.GetAll(searchQuery: searchQuery).ToList();

        Assert.Equal(new[] { first.Id, second.Id }, results.Select(entity => entity.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAll_Orders_By_Name(bool ascending)
    {
        var first = CreateEntity("Alpha");
        var second = CreateEntity("Beta");
        var third = CreateEntity("Gamma");
        SetupEntities(second, third, first);

        var results = facade.GetAll(ascending: ascending).ToList();

        Guid[] expected = ascending ? [first.Id, second.Id, third.Id] : [third.Id, second.Id, first.Id];
        Assert.Equal(expected, results.Select(entity => entity.Id));
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Description")]
    
    public void GetAll_Searches_Supported_Fields(string field)
    {
        var matching = CreateEntity();
        switch (field)
        {
            case "Name": matching.Name = "Prefix needle suffix"; break;
            case "Description": matching.Description = "Prefix needle suffix"; break;
            
        }
        SetupEntities(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(searchQuery: "needle"));

        Assert.Equal(matching.Id, result.Id);
    }

    [Theory]
    [InlineData("ačs")]
    [InlineData("AČS")]
    [InlineData("Ačs")]
    public void GetAll_Searches_Name_Ignoring_Case_With_Null_Description(string searchQuery)
    {
        var matching = CreateEntity("Some place with diacritis ačs") with { Description = null };
        SetupEntities(CreateEntity("Other") with { Description = null }, matching);

        var result = Assert.Single(facade.GetAll(searchQuery: searchQuery));

        Assert.Equal(matching.Id, result.Id);
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_When_Search_Does_Not_Match()
    {
        SetupEntities(CreateEntity() with { Description = null });

        Assert.Empty(facade.GetAll(searchQuery: "missing"));
    }

    [Fact]
    public void GetAll_Filters_By_Author_Id()
    {
        var matching = CreateEntity();
        // Filtering uses the foreign key even when the navigation is not loaded.
        matching.Author = null;
        SetupEntities(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(authorId: matching.AuthorId));

        Assert.Equal(matching.Id, result.Id);
    }

    [Fact]
    public void GetAll_Returns_Empty_Collection_For_Unknown_Author()
    {
        SetupEntities(CreateEntity());

        Assert.Empty(facade.GetAll(authorId: Guid.NewGuid()));
    }

    [Fact]
    public void GetAll_Returns_Mapped_List_Model()
    {
        var entity = CreateEntity();
        SetupEntities(entity);

        var result = Assert.Single(facade.GetAll());

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.ImageUrl, result.ImageUrl);
        repositoryMock.Verify(repository => repository.GetAll(), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Maps_Null_Optional_Fields_And_Empty_Items()
    {
        var entity = CreateEntity() with { ImageUrl = null, Description = null, Author = null, Items = [] };
        repositoryMock.Setup(repository => repository.GetById(entity.Id)).Returns(entity);

        var result = facade.GetById(entity.Id);

        Assert.NotNull(result);
        AssertDetailFields(entity, result);
    }

    [Theory]
    [InlineData(1, 2, new[] { "Alpha", "Beta" })]
    [InlineData(2, 2, new[] { "Delta", "Gamma" })]
    [InlineData(3, 2, new[] { "Omega" })]
    [InlineData(4, 2, new string[0])]
    public void GetAll_Returns_Requested_Page(int page, int pageSize, string[] expected)
    {
        SetupEntities(CreateEntity("Omega"), CreateEntity("Beta"), CreateEntity("Gamma"),
            CreateEntity("Alpha"), CreateEntity("Delta"));

        var results = facade.GetAll(page: page, pageSize: pageSize).ToList();

        Assert.Equal(expected, results.Select(entity => entity.Name));
    }

    [Fact]
    public void GetAll_Default_Page_Returns_First_25_Entities_In_Name_Order()
    {
        var entities = Enumerable.Range(1, 30).Select(index => CreateEntity($"Place {index:D2}")).ToArray();
        SetupEntities(entities.Reverse().ToArray());

        var results = facade.GetAll().ToList();

        Assert.Equal(entities.Take(25).Select(entity => entity.Id), results.Select(entity => entity.Id));
    }

    [Fact]
    public void GetAll_Applies_Search_Author_And_Descending_Order_Before_Paging()
    {
        var first = CreateEntity("Alpha match");
        var second = CreateEntity("Beta match") with { AuthorId = first.AuthorId, Author = first.Author };
        var third = CreateEntity("Gamma match") with { AuthorId = first.AuthorId, Author = first.Author };
        var wrongAuthor = CreateEntity("Delta match");
        var wrongSearch = first with { Id = Guid.NewGuid(), Name = "Omega" };
        SetupEntities(wrongAuthor, first, wrongSearch, second, third);

        var result = Assert.Single(facade.GetAll(searchQuery: "match", authorId: first.AuthorId,
            ascending: false, page: 2, pageSize: 1));

        Assert.Equal(second.Id, result.Id);
    }

    private static PlaceEntity CreateEntity(string name = "Pantry")
    {
        var author = FacadeTestData.CreateUser();
        return new PlaceEntity
        {
            Id = Guid.NewGuid(), Name = name, ImageUrl = "pantry.png", Description = "Food storage",
            AuthorId = author.Id, Author = author, Items = [FacadeTestData.CreateItem()]
        };
    }

    private static PlaceDetailModel CreateDetailModel()
    {
        var entity = CreateEntity();
        return new PlaceDetailModel
        {
            Id = entity.Id, Name = entity.Name, ImageUrl = entity.ImageUrl, Description = entity.Description,
            Author = FacadeTestData.ToListModel(entity.Author),
            Items = entity.Items.Select(item => FacadeTestData.ToListModel(item)!).ToList()
        };
    }

    private static void AssertDetailFields(PlaceEntity entity, PlaceDetailModel model)
    {
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Name, model.Name);
        Assert.Equal(entity.ImageUrl, model.ImageUrl);
        Assert.Equal(entity.Description, model.Description);
        Assert.Equivalent(FacadeTestData.ToListModel(entity.Author), model.Author, strict: true);
        Assert.Equivalent(entity.Items.Select(item => FacadeTestData.ToListModel(item)), model.Items, strict: true);
    }

    private static void AssertSavedEntity(PlaceDetailModel model, PlaceEntity entity)
    {
        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Name, entity.Name);
        Assert.Equal(model.ImageUrl, entity.ImageUrl);
        Assert.Equal(model.Description, entity.Description);
        Assert.Equal(model.Author!.Id, entity.AuthorId);
    }
}

