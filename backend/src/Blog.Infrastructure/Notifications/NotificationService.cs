using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
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
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                RelatedManuscriptId = relatedManuscriptId,
                CreatedAtUtc = createdAtUtc,
                IsRead = false
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyUsersWithPermissionAsync(
        string permissionCode,
        string title,
        string message,
        int? relatedManuscriptId = null,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Where(u => u.UserRoles.Any(ur =>
                ur.Role != null &&
                ur.Role.RolePermissions.Any(rp =>
                    rp.Permission != null && rp.Permission.Code == permissionCode)));

        if (excludeUserId is int excluded)
        {
            query = query.Where(u => u.Id != excluded);
        }

        var userIds = await query
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        await NotifyUsersAsync(userIds, title, message, relatedManuscriptId, cancellationToken);
    }
}
