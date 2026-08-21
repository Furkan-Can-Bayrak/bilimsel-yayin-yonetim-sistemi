using Blog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository. Tüm entity'ler için ortak CRUD.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly BlogDbContext Db;
    protected readonly DbSet<TEntity> Set;

    public Repository(BlogDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync([id], cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        // FindAsync cache'e bakar; Exists için DB sorgusu daha doğru.
        // PK adı Id varsayımı: tüm entity'lerimizde Id int.
        return Set.AnyAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        Set.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        Set.Remove(entity);
    }
}
