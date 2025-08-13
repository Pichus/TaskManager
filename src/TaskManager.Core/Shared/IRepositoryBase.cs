namespace TaskManager.Core.Shared;

public interface IRepositoryBase<TEntity, TId>
    where TEntity : class
{
    Task<TEntity?> FindByIdAsync(TId id);

    void Create(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}