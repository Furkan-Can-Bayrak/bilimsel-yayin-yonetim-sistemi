using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetByIdForUserAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> ListForUserAsync(
        int userId,
        int take,
        CancellationToken cancellationToken = default);
}
