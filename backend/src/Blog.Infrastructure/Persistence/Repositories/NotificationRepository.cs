using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(BlogDbContext db)
        : base(db)
    {
    }

    public Task<Notification?> GetByIdForUserAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListForUserPagedAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenByDescending(n => n.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> CountUnreadForUserAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        Set.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
}
