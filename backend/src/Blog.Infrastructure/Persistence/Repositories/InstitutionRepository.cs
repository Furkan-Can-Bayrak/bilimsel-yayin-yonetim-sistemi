using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class InstitutionRepository : Repository<Institution>, IInstitutionRepository
{
    public InstitutionRepository(BlogDbContext db)
        : base(db)
    {
    }

    public async Task<IReadOnlyList<Institution>> ListOrderedByNameAsync(
        CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking()
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }
}
