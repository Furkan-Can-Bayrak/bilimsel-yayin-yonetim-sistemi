namespace Blog.Application.Notifications.Dtos;

public sealed record NotificationDto(
    int Id,
    string Title,
    string Message,
    int? RelatedManuscriptId,
    DateTime CreatedAtUtc,
    bool IsRead);
