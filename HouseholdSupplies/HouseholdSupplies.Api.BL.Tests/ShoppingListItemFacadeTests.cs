using HouseholdSupplies.Api.BL.Facades;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Models.ShoppingListItem;
using Moq;

namespace HouseholdSupplies.Api.BL.Tests;

public class ShoppingListItemFacadeTests
{
    private readonly Mock<IShoppingListItemRepository> repositoryMock = new(MockBehavior.Strict);
    private readonly ShoppingListItemFacade facade;

    public ShoppingListItemFacadeTests()
    {
        facade = new ShoppingListItemFacade(repositoryMock.Object, new ShoppingListItemMapper());
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
        repositoryMock.Setup(repository => repository.GetById(id)).Returns((ShoppingListItemEntity?)null);

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
        ShoppingListItemEntity? saved = null;
        if (update)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<ShoppingListItemEntity>()))
                .Callback<ShoppingListItemEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<ShoppingListItemEntity>()))
                .Callback<ShoppingListItemEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = update ? facade.Update(model, []) : facade.Create(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertDetailFields(saved, model);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ShoppingListItemEntity>()), update ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ShoppingListItemEntity>()), update ? Times.Never() : Times.Once());
        repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Update_Returns_Null_For_Missing_Entity()
    {
        var model = CreateDetailModel();
        repositoryMock.Setup(repository => repository.Update(It.Is<ShoppingListItemEntity>(entity => entity.Id == model.Id)))
            .Returns((Guid?)null);

        Assert.Null(facade.Update(model, []));

        repositoryMock.Verify(repository => repository.Update(It.Is<ShoppingListItemEntity>(entity => entity.Id == model.Id)), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateOrUpdate_Selects_Correct_Repository_Operation(bool exists)
    {
        var model = CreateDetailModel();
        var savedId = Guid.NewGuid();
        ShoppingListItemEntity? saved = null;
        repositoryMock.Setup(repository => repository.Exists(model.Id)).Returns(exists);
        if (exists)
        {
            repositoryMock.Setup(repository => repository.Update(It.IsAny<ShoppingListItemEntity>()))
                .Callback<ShoppingListItemEntity>(entity => saved = entity).Returns(savedId);
        }
        else
        {
            repositoryMock.Setup(repository => repository.Insert(It.IsAny<ShoppingListItemEntity>()))
                .Callback<ShoppingListItemEntity>(entity => saved = entity).Returns(savedId);
        }

        var result = facade.CreateOrUpdate(model, []);

        Assert.Equal(savedId, result);
        Assert.NotNull(saved);
        AssertDetailFields(saved, model);
        repositoryMock.Verify(repository => repository.Exists(model.Id), Times.Once);
        repositoryMock.Verify(repository => repository.Update(It.IsAny<ShoppingListItemEntity>()), exists ? Times.Once() : Times.Never());
        repositoryMock.Verify(repository => repository.Insert(It.IsAny<ShoppingListItemEntity>()), exists ? Times.Never() : Times.Once());
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

    private void SetupEntities(params ShoppingListItemEntity[] entities)
    {
        repositoryMock.Setup(repository => repository.GetAll()).Returns(entities.AsQueryable());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetAll_Returns_Mapped_List_Model(bool isBought)
    {
        var entity = CreateEntity() with { IsBought = isBought };
        SetupEntities(entity);

        var result = Assert.Single(facade.GetAll());

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Amount, result.Amount);
        Assert.Equal(entity.IsBought, result.IsBought);
        Assert.Equal(entity.ItemId, result.ItemId);
        Assert.Equivalent(entity.Item, result.Item, strict: true);
        repositoryMock.Verify(repository => repository.GetAll(), Times.Once);
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public void GetAll_Filters_By_IsBought(bool? isBought)
    {
        var bought = CreateEntity() with { IsBought = true };
        var pending = CreateEntity() with { IsBought = false };
        var anotherBought = CreateEntity() with { IsBought = true };
        SetupEntities(bought, pending, anotherBought);

        var results = facade.GetAll(isBought: isBought).ToList();

        Guid[] expected = isBought switch
        {
            true => [bought.Id, anotherBought.Id],
            false => [pending.Id],
            null => [bought.Id, pending.Id, anotherBought.Id]
        };
        Assert.Equal(expected.Order(), results.Select(entity => entity.Id).Order());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetAll_Returns_Empty_Collection_When_IsBought_Does_Not_Match(bool isBought)
    {
        SetupEntities(CreateEntity() with { IsBought = !isBought });

        Assert.Empty(facade.GetAll(isBought: isBought));
    }
    
    private static ShoppingListItemEntity CreateEntity() => FacadeTestData.CreateShoppingListItem();

    private static ShoppingListItemDetailModel CreateDetailModel()
    {
        var entity = CreateEntity();
        return new ShoppingListItemDetailModel
        {
            Id = entity.Id, Amount = entity.Amount, IsBought = entity.IsBought,
            ItemId = entity.ItemId, Item = entity.Item,
            ShoppingListId = entity.ShoppingListId, ShoppingList = entity.ShoppingList
        };
    }

    private static void AssertDetailFields(ShoppingListItemEntity entity, ShoppingListItemDetailModel model)
    {
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Amount, model.Amount);
        Assert.Equal(entity.IsBought, model.IsBought);
        Assert.Equal(entity.ItemId, model.ItemId);
        Assert.Equal(entity.ShoppingListId, model.ShoppingListId);
        Assert.Equivalent(entity.Item, model.Item, strict: true);
        Assert.Equivalent(entity.ShoppingList, model.ShoppingList, strict: true);
    }
}

