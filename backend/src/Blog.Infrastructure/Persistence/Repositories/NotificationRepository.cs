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

    public async Task<IReadOnlyList<Notification>> ListForUserAsync(
        int userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
