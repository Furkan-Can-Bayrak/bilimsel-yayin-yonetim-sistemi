using Blog.Application.Common.Interfaces;
using Blog.Domain.Enums;
using MediatR;

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
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;

    public GetMyReviewsQueryHandler(IReviewRepository reviews, ICurrentUser currentUser)
    {
        _reviews = reviews;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MyReviewListItemDto>> Handle(
        GetMyReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviewerId = _currentUser.RequireUserId();
        var reviews = await _reviews.ListByReviewerAsync(reviewerId, cancellationToken);

        return reviews
            .Select(r => new MyReviewListItemDto(
                r.Id,
                r.ManuscriptId,
                r.Manuscript?.Title ?? string.Empty,
                r.Manuscript?.Status ?? ManuscriptStatus.Draft,
                r.AssignedAtUtc,
                r.SubmittedAtUtc,
                r.Recommendation))
            .ToList();
    }
}
