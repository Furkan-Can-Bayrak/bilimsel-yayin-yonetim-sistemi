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

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await Db.Roles
            .Where(r => r.UserRoles.Any(ur => ur.UserId == userId))
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await Db.Permissions
            .Where(p => p.RolePermissions.Any(rp =>
                rp.Role != null &&
                rp.Role.UserRoles.Any(ur => ur.UserId == userId)))
            .OrderBy(p => p.Code)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> ListPagedWithRolesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await Set.CountAsync(cancellationToken);

        var items = await Set.AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
