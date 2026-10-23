using HouseholdSupplies.Api.App.Filters;
using HouseholdSupplies.Api.App.Extensions;
using HouseholdSupplies.Api.App.Models;
using Microsoft.AspNetCore.Mvc;
using HouseholdSupplies.Api.App.Resources.Texts;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.ShoppingList;
using HouseholdSupplies.Common.Models.ShoppingListItem;
using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Api.App.Endpoints;

public class ItemEndpoints : EndpointsBase
{
    public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var endpoints = endpointRouteBuilder.MapGroup("item").WithTags("item")
            .AddEndpointFilter<PersistenceExceptionFilter>();

        endpoints.MapGet("", (IItemFacade facade, string? searchQuery = null, bool ascending = true, Guid? placeId = null,
            string? categoryName = null, int page = 1, int pageSize = 25) =>
        {
            if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
                return Results.BadRequest(new ApiErrorResponse(ItemEndpointsResources.Paging));

            var categoryFilter = nameof(Category);
            if (categoryName is not null)
            {
                if (!Enum.TryParse<Category>(categoryName, true, out var category) || !Enum.IsDefined(category))
                    return Results.BadRequest(new ApiErrorResponse(ItemEndpointsResources.Category));
                categoryFilter = ((int)category).ToString();
            }

            return Results.Ok(facade.GetAll(searchQuery, ascending, placeId, categoryFilter, page, pageSize));
        })
            .ProducesResponse<IEnumerable<ItemListModel>>(200, "A filtered, sorted page of items.")
            .ProducesResponse<ApiErrorResponse>(400, "Invalid pagination, category filter, or query parameter format.");

        endpoints.MapGet("{id:guid}", (Guid id, IItemFacade facade) =>
            facade.GetById(id) is { } item ? Results.Ok(item) : Results.NotFound())
            .ProducesResponse<ItemDetailModel>(200, "The item detail, including its author and storage place.")
            .ProducesResponse(404, "The item does not exist.");

        endpoints.MapPost("", (ItemDetailModel item, IItemFacade facade, IHttpContextAccessor context) =>
            Results.Ok(facade.Create(item with { Id = Guid.NewGuid() }, GetUserRoles(context), GetUserId(context))))
            .AddEndpointFilter<ValidationFilter<ItemDetailModel>>()
            .ProducesResponse<Guid>(200, "The created item ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author or place does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The item failed input validation.", "application/problem+json");

        endpoints.MapPut("", (ItemDetailModel item, IItemFacade facade, IItemRepository items, IHttpContextAccessor context) =>
        {
            // Check existence before ownership so a missing record always returns 404.
            if (!items.Exists(item.Id))
                return Results.NotFound();
            try
            {
                var id = facade.Update(item, GetUserRoles(context), GetUserId(context));
                return id.HasValue ? Results.Ok(id.Value) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<ItemDetailModel>>()
            .ProducesResponse<Guid>(200, "The updated item ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author or place does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this item.", "application/problem+json")
            .ProducesResponse(404, "The item does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The item failed input validation.", "application/problem+json");

        endpoints.MapPost("save", (ItemDetailModel item, IItemFacade facade, IHttpContextAccessor context) =>
        {
            try
            {
                if (item.Id == Guid.Empty)
                    item = item with { Id = Guid.NewGuid() };
                return Results.Ok(facade.CreateOrUpdate(item, GetUserRoles(context), GetUserId(context)));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        }).AddEndpointFilter<ValidationFilter<ItemDetailModel>>()
            .ProducesResponse<Guid>(200, "The created or updated item ID.")
            .ProducesResponse<HttpValidationProblemDetails>(400, "The request body is missing or malformed, or a referenced author or place does not exist.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot modify this item.", "application/problem+json")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json")
            .ProducesResponse<HttpValidationProblemDetails>(422, "The item failed input validation.", "application/problem+json");

        endpoints.MapDelete("{id:guid}", (Guid id, IItemFacade facade, IHttpContextAccessor context) =>
        {
            if (facade.GetById(id) is null)
                return Results.NotFound();
            try
            {
                facade.Delete(id, GetUserRoles(context), GetUserId(context));
                return Results.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }
        })
            .ProducesResponse(200, "The item was deleted.")
            .ProducesResponse<ProblemDetails>(403, "The caller cannot delete this item.", "application/problem+json")
            .ProducesResponse(404, "The item does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json");

        endpoints.MapPost("{id:guid}/shopping-list/{shoppingListId:guid}",
            (Guid id, Guid shoppingListId, AddToShoppingListRequest request,
                IItemFacade items, IShoppingListFacade lists, IShoppingListItemFacade entries) =>
            {
                if (!double.IsFinite(request.Amount) || request.Amount <= 0)
                    return Results.BadRequest(new ApiErrorResponse(ItemEndpointsResources.Amount));
                var item = items.GetById(id);
                if (item is null)
                    return Results.NotFound();
                var list = lists.GetById(shoppingListId);
                if (list is null)
                    return Results.BadRequest(new ApiErrorResponse(ItemEndpointsResources.ShoppingListNotFound));

                var entry = new ShoppingListItemDetailModel
                {
                    Id = Guid.NewGuid(), Amount = request.Amount, IsBought = false,
                    Item = new ItemListModel
                    {
                        Id = item.Id, Name = item.Name, ImageUrl = item.ImageUrl,
                        Amount = item.Amount, Category = item.Category, Place = item.Place
                    },
                    ShoppingList = new ShoppingListListModel
                    {
                        Id = list.Id, Name = list.Name, Shop = list.Shop
                    }
                };
                // Adding an entry does not require ownership of the shopping list.
                return Results.Ok(entries.Create(entry, []));
            })
            .ProducesResponse<Guid>(200, "The created shopping-list entry ID.")
            .ProducesResponse<ApiErrorResponse>(400, "The request body is missing or malformed, the amount is invalid, or the shopping list does not exist.")
            .ProducesResponse(404, "The item does not exist.")
            .ProducesResponse<ProblemDetails>(409, "The operation conflicts with existing records.", "application/problem+json");

        return endpointRouteBuilder;
    }

    private static IResult Forbidden() =>
        Results.Problem(statusCode: StatusCodes.Status403Forbidden,
            title: ItemEndpointsResources.Forbidden_Title, detail: ItemEndpointsResources.Forbidden_Detail);
}

public record AddToShoppingListRequest(double Amount);
