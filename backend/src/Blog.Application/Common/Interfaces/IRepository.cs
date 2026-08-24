namespace Blog.Application.Common.Interfaces;

/// <summary>
/// Ortak CRUD. Entity özel sorgular için IXxxRepository : IRepository&lt;T&gt; açılır.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Remove(TEntity entity);
}
