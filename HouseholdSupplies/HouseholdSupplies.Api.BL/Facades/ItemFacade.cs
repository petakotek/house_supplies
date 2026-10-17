using HouseholdSupplies.Api.BL.Facades.Interfaces;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Common.Entities;
using HouseholdSupplies.Api.DAL.Common.Repositories;
using HouseholdSupplies.Common.Enums;
using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Api.BL.Facades;

public class ItemFacade(IItemRepository itemRepository, ItemMapper mapper)
    : FacadeBase<IItemRepository, ItemEntity>(itemRepository), IItemFacade
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
            query = query.Where(item => item.Author != null && item.Place != null && (
                item.Name.Contains(searchQuery) ||
                item.Description.Contains(searchQuery) ||
                item.Author.Name.Contains(searchQuery) ||
                item.Place.Name.Contains(searchQuery)
                ));
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

    public ItemDetailModel? GetById(Guid id)
    {
        var itemEntity = _itemRepository.GetById(id);
        if (itemEntity == null)
        {
            return null;
        }
        return _mapper.ToDetailModel(itemEntity);
    }

    public Guid CreateOrUpdate(ItemDetailModel model, IList<string> userRoles, string? ownerId = null)
    {
        return _itemRepository.Exists(model.Id)
            ? Update(model, userRoles, ownerId)!.Value
            : Create(model, userRoles, ownerId);
    }

    public Guid Create(ItemDetailModel itemModel, IList<string> userRoles, string? ownerId = null)
    {
        var itemEntity = _mapper.ToEntity(itemModel, ownerId);
        return _itemRepository.Insert(itemEntity);
    }

    public Guid? Update(ItemDetailModel itemModel, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(itemModel.Id, userRoles, ownerId);

        var itemEntity = _mapper.ToEntity(itemModel, ownerId);
        return _itemRepository.Update(itemEntity);
    }

    public void Delete(Guid id, IList<string> userRoles, string? ownerId = null)
    {
        ThrowIfWrongOwnerAndNotAdmin(id, userRoles, ownerId);
        _itemRepository.Remove(id);
    }
}