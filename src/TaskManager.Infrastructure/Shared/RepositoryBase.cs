using TaskManager.Core.Shared;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Shared;

public abstract class RepositoryBase<TEntity, TId> : IRepositoryBase<TEntity, TId>
    where TEntity : class
{
    protected readonly AppDbContext Context;

    protected RepositoryBase(AppDbContext context)
    {
        Context = context;
    }

    public async Task<TEntity?> FindByIdAsync(TId id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }

    public void Create(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        Context.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }
}