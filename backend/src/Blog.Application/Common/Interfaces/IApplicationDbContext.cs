using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Post> Posts { get; }
    DbSet<Category> Categories { get; }
    DbSet<User> Users { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
