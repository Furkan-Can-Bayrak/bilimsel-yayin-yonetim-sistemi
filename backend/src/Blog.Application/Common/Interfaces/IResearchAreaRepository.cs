using Blog.Application.Common.Models;
using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IResearchAreaRepository : IRepository<ResearchArea>
{
    /// <summary>Verilen slug başka bir kayıtta kullanılıyor mu?</summary>
    Task<bool> SlugExistsAsync(
        string slug,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<ResearchArea?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResearchAreaWithCount>> ListWithManuscriptCountsAsync(
        CancellationToken cancellationToken = default);

    Task<ResearchAreaWithCount?> GetWithManuscriptCountAsync(
        int id,
        CancellationToken cancellationToken = default);
}
