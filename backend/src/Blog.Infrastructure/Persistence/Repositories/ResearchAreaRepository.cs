using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
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

    public async Task<IReadOnlyList<ResearchAreaDto>> ListWithManuscriptCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ResearchAreaDto(
                a.Id,
                a.Name,
                a.Slug,
                a.Manuscripts.Count))
            .ToListAsync(cancellationToken);
    }

    public Task<ResearchAreaDto?> GetWithManuscriptCountAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ResearchAreaDto(
                a.Id,
                a.Name,
                a.Slug,
                a.Manuscripts.Count))
            .FirstOrDefaultAsync(cancellationToken);
}
