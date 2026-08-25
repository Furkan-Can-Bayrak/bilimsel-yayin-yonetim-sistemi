using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetByIdForUserAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListForUserPagedAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadForUserAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
