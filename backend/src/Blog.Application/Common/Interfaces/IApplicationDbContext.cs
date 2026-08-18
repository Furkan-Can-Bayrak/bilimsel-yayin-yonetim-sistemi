using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Manuscript> Manuscripts { get; }
    DbSet<ResearchArea> ResearchAreas { get; }
    DbSet<User> Users { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
