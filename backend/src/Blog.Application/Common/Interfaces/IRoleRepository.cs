using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task<int> CountByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> ListOrderedByNameAsync(
        CancellationToken cancellationToken = default);
}
