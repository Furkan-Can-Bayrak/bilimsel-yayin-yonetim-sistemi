namespace Blog.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(string title, string message, int? relatedPostId = null, CancellationToken cancellationToken = default);
}
