using HouseholdSupplies.Api.BL.Facades;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.ShoppingList;
using Moq;

namespace HouseholdSupplies.Api.BL.Tests;

public class ShoppingListFacadeTests
{
    private readonly Mock<IShoppingListRepository> repositoryMock = new(MockBehavior.Strict);
    private readonly ShoppingListFacade facade;

    public ShoppingListFacadeTests()
    {
        facade = new ShoppingListFacade(repositoryMock.Object, new ShoppingListMapper());
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
        repositoryMock.Setup(repository => repository.GetById(id)).Returns((ShoppingListEntity?)null);

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
        ShoppingListEntity? saved = null;
        if (update)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<ShoppingListEntity>()))
                .Callback<ShoppingListEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<ShoppingListEntity>()))
                .Callback<ShoppingListEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = update ? facade.Update(model, []) : facade.Create(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertSavedEntity(model, saved);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ShoppingListEntity>()), update ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ShoppingListEntity>()), update ? Times.Never() : Times.Once());
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_Entity()
    {
        var model = CreateDetailModel();
        repositoryMock.Setup(repository => repository.Update(It.Is<ShoppingListEntity>(entity => entity.Id == model.Id)))
            .Returns((Guid?)null);

        Assert.Null(facade.Update(model, []));

        repositoryMock.Verify(repository => repository.Update(It.Is<ShoppingListEntity>(entity => entity.Id == model.Id)), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateOrUpdate_Selects_Correct_Repository_Operation(bool exists)
    {
        var model = CreateDetailModel();
        var savedId = Guid.NewGuid();
        ShoppingListEntity? saved = null;
        repositoryMock.Setup(repository => repository.Exists(model.Id)).Returns(exists);
        if (exists)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<ShoppingListEntity>()))
                .Callback<ShoppingListEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<ShoppingListEntity>()))
                .Callback<ShoppingListEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = facade.CreateOrUpdate(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertSavedEntity(model, saved);
        repositoryMock.Verify(repository => repository.Exists(model.Id), Times.Once);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ShoppingListEntity>()), exists ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ShoppingListEntity>()), exists ? Times.Never() : Times.Once());
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

    private void SetupEntities(params ShoppingListEntity[] entities)
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
    [InlineData("Shop")]
    public void GetAll_Searches_Supported_Fields(string field)
    {
        var matching = CreateEntity();
        switch (field)
        {
            case "Name": matching.Name = "Prefix needle suffix"; break;
            case "Description": matching.Description = "Prefix needle suffix"; break;
            case "Shop": matching.Shop = "Prefix needle suffix"; break;
        }
        SetupEntities(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(searchQuery: "needle"));

        Assert.Equal(matching.Id, result.Id);
    }

    [Theory]
    [InlineData("šunka")]
    [InlineData("ŠUNKA")]
    [InlineData("Šunka")]
    public void GetAll_Searches_Name_Ignoring_Case_With_Null_Description(string searchQuery)
    {
        var matching = CreateEntity("Čerstvá ŠUNKA") with { Description = null };
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
        Assert.Equal(entity.Shop, result.Shop);
        repositoryMock.Verify(repository => repository.GetAll(), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetById_Maps_Null_Optional_Fields_And_Empty_Items()
    {
        var entity = CreateEntity() with { Description = null, Author = null, Items = [] };
        repositoryMock.Setup(repository => repository.GetById(entity.Id)).Returns(entity);

        var result = facade.GetById(entity.Id);

        Assert.NotNull(result);
        AssertDetailFields(entity, result);
    }

    [Theory]
    [InlineData("Description", "šunka")]
    [InlineData("Description", "ŠUNKA")]
    [InlineData("Shop", "šunka")]
    [InlineData("Shop", "ŠUNKA")]
    public void GetAll_Searches_Description_And_Shop_Ignoring_Case(string field, string searchQuery)
    {
        var matching = CreateEntity();
        if (field == "Description")
            matching.Description = "Čerstvá Šunka";
        else
            matching.Shop = "Čerstvá Šunka";
        SetupEntities(CreateEntity(), matching);

        var result = Assert.Single(facade.GetAll(searchQuery: searchQuery));

        Assert.Equal(matching.Id, result.Id);
    }

    [Fact]
    public void GetAll_Combines_Search_And_Author_And_Orders_Descending()
    {
        var first = CreateEntity("Alpha match");
        var second = CreateEntity("Beta match") with { AuthorId = first.AuthorId, Author = first.Author };
        var wrongAuthor = CreateEntity("Gamma match");
        var wrongSearch = first with { Id = Guid.NewGuid(), Name = "Omega" };
        SetupEntities(wrongAuthor, first, wrongSearch, second);

        var results = facade.GetAll(searchQuery: "match", authorId: first.AuthorId, ascending: false).ToList();

        Assert.Equal(new[] { second.Id, first.Id }, results.Select(entity => entity.Id));
    }

    private static ShoppingListEntity CreateEntity(string name = "Weekly shopping")
    {
        var author = FacadeTestData.CreateUser();
        return new ShoppingListEntity
        {
            Id = Guid.NewGuid(), Name = name, Shop = "Market", Description = "Groceries",
            AuthorId = author.Id, Author = author, Items = [FacadeTestData.CreateShoppingListItem()]
        };
    }

    private static ShoppingListDetailModel CreateDetailModel()
    {
        var entity = CreateEntity();
        return new ShoppingListDetailModel
        {
            Id = entity.Id, Name = entity.Name, Shop = entity.Shop, Description = entity.Description,
            Author = FacadeTestData.ToListModel(entity.Author),
            Items = entity.Items.Select(item => FacadeTestData.ToListModel(item)!).ToList()
        };
    }

    private static void AssertDetailFields(ShoppingListEntity entity, ShoppingListDetailModel model)
    {
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Name, model.Name);
        Assert.Equal(entity.Shop, model.Shop);
        Assert.Equal(entity.Description, model.Description);
        Assert.Equivalent(FacadeTestData.ToListModel(entity.Author), model.Author, strict: true);
        Assert.Equivalent(entity.Items.Select(item => FacadeTestData.ToListModel(item)), model.Items, strict: true);
    }

    private static void AssertSavedEntity(ShoppingListDetailModel model, ShoppingListEntity entity)
    {
        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Name, entity.Name);
        Assert.Equal(model.Shop, entity.Shop);
        Assert.Equal(model.Description, entity.Description);
        Assert.Equal(model.Author!.Id, entity.AuthorId);
    }
}

