using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class ManuscriptRepository : Repository<Manuscript>, IManuscriptRepository
{
    public ManuscriptRepository(BlogDbContext db)
        : base(db)
    {
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsQueryable();

        if (excludeId is int id)
        {
            query = query.Where(m => m.Id != id);
        }

        return query.AnyAsync(m => m.Slug == slug, cancellationToken);
    }

    public Task<Manuscript?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(m => m.Slug == slug, cancellationToken);

    public Task<bool> AnyInResearchAreaAsync(
        int researchAreaId,
        CancellationToken cancellationToken = default) =>
        Set.AnyAsync(m => m.ResearchAreaId == researchAreaId, cancellationToken);

    public async Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListPublishedPagedAsync(
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyListFilters(
            Set.AsNoTracking().Where(m => m.Status == ManuscriptStatus.Published),
            search,
            researchAreaId,
            status: null);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(m => m.Author)
            .Include(m => m.ResearchArea)
            .OrderByDescending(m => m.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Manuscript?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.ResearchArea)
            .FirstOrDefaultAsync(
                m => m.Status == ManuscriptStatus.Published && m.Slug == slug,
                cancellationToken);

    public Task<Manuscript?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.ResearchArea)
            .Include(m => m.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListEditorialPagedAsync(
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        ManuscriptStatus? status,
        int editorUserId,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(m => m.AuthorId != editorUserId && m.Status != ManuscriptStatus.Draft);

        return ListAdminPagedAsync(query, page, pageSize, search, researchAreaId, status, cancellationToken);
    }

    public Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListMinePagedAsync(
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        ManuscriptStatus? status,
        int authorId,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().Where(m => m.AuthorId == authorId);
        return ListAdminPagedAsync(query, page, pageSize, search, researchAreaId, status, cancellationToken);
    }

    private async Task<(IReadOnlyList<Manuscript> Items, int TotalCount)> ListAdminPagedAsync(
        IQueryable<Manuscript> query,
        int page,
        int pageSize,
        string? search,
        int? researchAreaId,
        ManuscriptStatus? status,
        CancellationToken cancellationToken)
    {
        query = ApplyListFilters(query, search, researchAreaId, status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(m => m.Author)
            .Include(m => m.ResearchArea)
            .Include(m => m.Reviews)
            .ThenInclude(r => r.Reviewer)
            .OrderBy(m =>
                m.Status == ManuscriptStatus.UnderReview
                    && m.Reviews.Any(r => r.SubmittedAtUtc != null)
                    ? 0 :
                m.Status == ManuscriptStatus.UnderReview ? 1 :
                m.Status == ManuscriptStatus.Submitted ? 2 :
                m.Status == ManuscriptStatus.Published ? 3 :
                m.Status == ManuscriptStatus.Accepted ? 4 :
                m.Status == ManuscriptStatus.Rejected ? 5 :
                6)
            .ThenByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Manuscript> ApplyListFilters(
        IQueryable<Manuscript> query,
        string? search,
        int? researchAreaId,
        ManuscriptStatus? status)
    {
        if (researchAreaId is int areaId)
        {
            query = query.Where(m => m.ResearchAreaId == areaId);
        }

        if (status is ManuscriptStatus manuscriptStatus)
        {
            query = query.Where(m => m.Status == manuscriptStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                m.Title.Contains(term) ||
                (m.Summary != null && m.Summary.Contains(term)));
        }

        return query;
    }
}
