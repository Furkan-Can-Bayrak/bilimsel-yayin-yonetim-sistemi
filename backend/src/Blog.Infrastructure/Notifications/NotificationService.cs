using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;

namespace Blog.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task NotifyAsync(
        string title,
        string message,
        int? relatedManuscriptId = null,
        CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(new Notification
        {
            Title = title,
            Message = message,
            RelatedManuscriptId = relatedManuscriptId,
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
