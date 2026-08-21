using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IManuscriptRepository : IRepository<Manuscript>
{
    Task<bool> SlugExistsAsync(
        string slug,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<Manuscript?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
