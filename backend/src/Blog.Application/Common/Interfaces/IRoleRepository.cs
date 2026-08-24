namespace Blog.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task<int> CountByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);
}
