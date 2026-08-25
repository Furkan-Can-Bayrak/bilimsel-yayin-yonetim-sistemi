using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Notifications.Dtos;
using Blog.Domain.Authorization;
using MediatR;

namespace Blog.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<NotificationDto>>;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private const int MaxPageSize = 50;
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

    public async Task<PagedResult<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var (items, totalCount) = await _notifications.ListForUserPagedAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        Dictionary<int, int>? reviewByManuscript = null;
        if (_currentUser.HasPermission(Permissions.Reviews.Submit))
        {
            var reviews = await _reviews.ListByReviewerAsync(userId, cancellationToken);
            reviewByManuscript = reviews
                .GroupBy(r => r.ManuscriptId)
                .ToDictionary(g => g.Key, g => g.First().Id);
        }

        var dtos = items
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.RelatedManuscriptId,
                RelatedReviewId(n.RelatedManuscriptId, reviewByManuscript),
                n.CreatedAtUtc,
                n.IsRead))
            .ToList();

        return new PagedResult<NotificationDto>(dtos, page, pageSize, totalCount);
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
