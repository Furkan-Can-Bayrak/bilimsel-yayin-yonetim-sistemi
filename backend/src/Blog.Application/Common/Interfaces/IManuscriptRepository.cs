using Blog.Domain.Entities;
using Blog.Domain.Enums;

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

    Task<bool> AnyInResearchAreaAsync(
        int researchAreaId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListPublishedPagedAsync(
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        CancellationToken cancellationToken = default);

    Task<Manuscript?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Manuscript?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListVisiblePagedAsync(
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        ManuscriptStatus? status,
        int? viewerUserId,
        bool canViewAll,
        CancellationToken cancellationToken = default);
}
