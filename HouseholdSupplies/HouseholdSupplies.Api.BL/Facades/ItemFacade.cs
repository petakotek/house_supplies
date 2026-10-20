using System.Globalization;
using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Entities;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Api.BL.Facades;

public class ItemFacade(IItemRepository itemRepository, ItemMapper mapper)
    : FacadeBase<IItemRepository, ItemEntity, ItemDetailModel, ItemListModel, ItemMapper>(itemRepository, mapper), IItemFacade
{
    private readonly IItemRepository _itemRepository = itemRepository;
    private readonly ItemMapper _mapper = mapper;

    public IEnumerable<ItemListModel> GetAll(
        string? searchQuery = null,
        bool ascending = true,
        Guid? placeId = null,
        string categoryName = nameof(Category),
        int page = 1,
        int pageSize = 25)
    {
        IQueryable<ItemEntity> query = _itemRepository.GetAll();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var normalizedSearchQuery = searchQuery.ToLowerInvariant();
            var comparison = StringComparison.InvariantCultureIgnoreCase;

            query = query.Where(item =>
                item.Name.Contains(normalizedSearchQuery, comparison) ||
                (item.Description != null && item.Description.Contains(normalizedSearchQuery, comparison)) ||
                (item.Author != null && item.Author.Name.Contains(normalizedSearchQuery, comparison)) ||
                (item.Place != null && item.Place.Name.Contains(normalizedSearchQuery, comparison)));
        }

        if (placeId.HasValue)
        {
            query = query.Where(item => item.PlaceId == placeId.Value);
        }

        if (int.TryParse(categoryName, out int categoryId))
        {
            query = query.Where(item => item.Category == (Category)categoryId);
        }

        query = ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);

        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        IEnumerable<ItemEntity> entities = query.ToList();
        return _mapper.ToListModels(entities);
    }
}
