namespace Blog.Application.Common.Interfaces;

public interface INotificationService
{
    /// <summary>Belirtilen kullanıcılara ayrı bildirim kaydı oluşturur.</summary>
    Task NotifyUsersAsync(
        IEnumerable<int> userIds,
        string title,
        string message,
        int? relatedManuscriptId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen izne sahip aktif kullanıcılara bildirir.
    /// <paramref name="excludeUserId"/> varsa o kullanıcı atlanır (ör. gönderen yazar).
    /// </summary>
    Task NotifyUsersWithPermissionAsync(
        string permissionCode,
        string title,
        string message,
        int? relatedManuscriptId = null,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default);
}
