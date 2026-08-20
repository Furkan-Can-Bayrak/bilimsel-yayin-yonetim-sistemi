using Blog.Application.Common.Interfaces;
using Blog.Application.Notifications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(int Take = 50) : IRequest<IReadOnlyList<NotificationDto>>;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var take = request.Take < 1 ? 50 : Math.Min(request.Take, 100);

        return await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(take)
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.RelatedManuscriptId,
                n.CreatedAtUtc,
                n.IsRead))
            .ToListAsync(cancellationToken);
    }
}
