using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;

namespace Blog.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public NotificationService(
        INotificationRepository notifications,
        IUserRepository users,
        IUnitOfWork uow)
    {
        _notifications = notifications;
        _users = users;
        _uow = uow;
    }

    public async Task NotifyUsersAsync(
        IEnumerable<int> userIds,
        string title,
        string message,
        int? relatedManuscriptId = null,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = userIds.Distinct().Where(id => id > 0).ToList();
        if (distinctIds.Count == 0)
        {
            return;
        }

        var createdAtUtc = DateTime.UtcNow;

        foreach (var userId in distinctIds)
        {
            await _notifications.AddAsync(
                new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    RelatedManuscriptId = relatedManuscriptId,
                    CreatedAtUtc = createdAtUtc,
                    IsRead = false
                },
                cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyUsersWithPermissionAsync(
        string permissionCode,
        string title,
        string message,
        int? relatedManuscriptId = null,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var userIds = await _users.ListActiveIdsByPermissionAsync(
            permissionCode,
            excludeUserId,
            cancellationToken);

        await NotifyUsersAsync(userIds, title, message, relatedManuscriptId, cancellationToken);
    }
}
