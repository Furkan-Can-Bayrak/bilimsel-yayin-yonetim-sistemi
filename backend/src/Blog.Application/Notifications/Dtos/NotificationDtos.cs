namespace Blog.Application.Notifications.Dtos;

public sealed record NotificationDto(
    int Id,
    string Title,
    string Message,
    int? RelatedPostId,
    DateTime CreatedAtUtc,
    bool IsRead);
