using Blog.Application.Common.Interfaces;
using Blog.Application.Notifications.Dtos;
using Blog.Domain.Authorization;
using MediatR;

namespace Blog.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(int Take = 50) : IRequest<IReadOnlyList<NotificationDto>>;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _notifications;
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(
        INotificationRepository notifications,
        IReviewRepository reviews,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _reviews = reviews;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var take = request.Take < 1 ? 50 : Math.Min(request.Take, 100);

        var items = await _notifications.ListForUserAsync(userId, take, cancellationToken);

        Dictionary<int, int>? reviewByManuscript = null;
        if (_currentUser.HasPermission(Permissions.Reviews.Submit))
        {
            var reviews = await _reviews.ListByReviewerAsync(userId, cancellationToken);
            reviewByManuscript = reviews
                .GroupBy(r => r.ManuscriptId)
                .ToDictionary(g => g.Key, g => g.First().Id);
        }

        return items
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.RelatedManuscriptId,
                RelatedReviewId(n.RelatedManuscriptId, reviewByManuscript),
                n.CreatedAtUtc,
                n.IsRead))
            .ToList();
    }

    private static int? RelatedReviewId(
        int? relatedManuscriptId,
        Dictionary<int, int>? reviewByManuscript)
    {
        if (relatedManuscriptId is int manuscriptId
            && reviewByManuscript is not null
            && reviewByManuscript.TryGetValue(manuscriptId, out var reviewId))
        {
            return reviewId;
        }

        return null;
    }
}
