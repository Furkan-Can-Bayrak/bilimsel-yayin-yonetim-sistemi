using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class ResearchAreaRepository : Repository<ResearchArea>, IResearchAreaRepository
{
    public ResearchAreaRepository(BlogDbContext db)
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
            query = query.Where(a => a.Id != id);
        }

        return query.AnyAsync(a => a.Slug == slug, cancellationToken);
    }

    public Task<ResearchArea?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
}
