using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BlogDbContext db)
        : base(db)
    {
    }

    public Task<User?> GetByIdWithRolesAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Set.Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        Set.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> OrcidExistsAsync(
        string orcid,
        CancellationToken cancellationToken = default) =>
        Set.IgnoreQueryFilters()
            .AnyAsync(u => u.Orcid == orcid, cancellationToken);

    public Task<bool> HasPermissionAsync(
        int userId,
        string permissionCode,
        CancellationToken cancellationToken = default) =>
        Set.Where(u => u.Id == userId)
            .AnyAsync(
                u => u.UserRoles.Any(ur =>
                    ur.Role != null &&
                    ur.Role.RolePermissions.Any(rp =>
                        rp.Permission != null &&
                        rp.Permission.Code == permissionCode)),
                cancellationToken);
}
