using Blog.Application.Common.Interfaces;
using MediatR;

namespace Blog.Application.Notifications.Queries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery : IRequest<int>;

public sealed class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUser _currentUser;

    public GetUnreadNotificationCountQueryHandler(
        INotificationRepository notifications,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _currentUser = currentUser;
    }

    public Task<int> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        return _notifications.CountUnreadForUserAsync(userId, cancellationToken);
    }
}
