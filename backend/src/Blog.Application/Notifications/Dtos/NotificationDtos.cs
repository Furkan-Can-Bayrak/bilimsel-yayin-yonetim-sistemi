namespace Blog.Application.Notifications.Dtos;

public sealed record NotificationDto(
    int Id,
    string Title,
    string Message,
    int? RelatedManuscriptId,
    int? RelatedReviewId,
    DateTime CreatedAtUtc,
    bool IsRead);
