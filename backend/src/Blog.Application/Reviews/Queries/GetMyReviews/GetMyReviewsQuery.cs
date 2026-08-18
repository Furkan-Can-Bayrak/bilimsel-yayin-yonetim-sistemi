using Blog.Application.Common.Interfaces;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Reviews.Queries.GetMyReviews;

public sealed record MyReviewListItemDto(
    int Id,
    int ManuscriptId,
    string ManuscriptTitle,
    ManuscriptStatus ManuscriptStatus,
    DateTime AssignedAtUtc,
    DateTime? SubmittedAtUtc,
    ReviewRecommendation? Recommendation);

public sealed record GetMyReviewsQuery : IRequest<IReadOnlyList<MyReviewListItemDto>>;

public sealed class GetMyReviewsQueryHandler
    : IRequestHandler<GetMyReviewsQuery, IReadOnlyList<MyReviewListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyReviewsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MyReviewListItemDto>> Handle(
        GetMyReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviewerId = _currentUser.RequireUserId();

        return await _db.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.AssignedAtUtc)
            .Select(r => new MyReviewListItemDto(
                r.Id,
                r.ManuscriptId,
                r.Manuscript != null ? r.Manuscript.Title : string.Empty,
                r.Manuscript != null ? r.Manuscript.Status : ManuscriptStatus.Draft,
                r.AssignedAtUtc,
                r.SubmittedAtUtc,
                r.Recommendation))
            .ToListAsync(cancellationToken);
    }
}
