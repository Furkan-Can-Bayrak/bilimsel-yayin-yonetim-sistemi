using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly BlogDbContext _db;

    public RoleRepository(BlogDbContext db)
    {
        _db = db;
    }

    public Task<int> CountByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default) =>
        _db.Roles.CountAsync(r => ids.Contains(r.Id), cancellationToken);
}
