namespace HouseholdSupplies.Api.BL.Mappers.Interfaces;

public interface IMapper<TEntity, out TListModel, TDetailModel>
{
    public TListModel ToListModel(TEntity entity);
    public TDetailModel ToDetailModel(TEntity entity);
    public TEntity ToEntity(TDetailModel model, string? ownerId = null);
    public IEnumerable<TListModel> ToListModels(IEnumerable<TEntity> entities);

}