using Blog.Application.Common.Interfaces;
using Blog.Application.Notifications.Dtos;
using MediatR;

namespace Blog.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(int Take = 50) : IRequest<IReadOnlyList<NotificationDto>>;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(
        INotificationRepository notifications,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var take = request.Take < 1 ? 50 : Math.Min(request.Take, 100);

        var items = await _notifications.ListForUserAsync(userId, take, cancellationToken);

        return items
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.RelatedManuscriptId,
                n.CreatedAtUtc,
                n.IsRead))
            .ToList();
    }
}
