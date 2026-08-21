using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
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
}
