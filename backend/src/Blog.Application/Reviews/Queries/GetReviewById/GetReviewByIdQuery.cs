using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Reviews.Queries.GetReviewById;

public sealed record ReviewDetailDto(
    int Id,
    int ManuscriptId,
    string ManuscriptTitle,
    string ManuscriptContent,
    string? ManuscriptSummary,
    int ReviewerId,
    string ReviewerName,
    DateTime AssignedAtUtc,
    DateTime? SubmittedAtUtc,
    ReviewSummaryDto Summary);

public sealed record GetReviewByIdQuery(int Id) : IRequest<ReviewDetailDto?>;

public sealed class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ReviewDetailDto?>
{
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;

    public GetReviewByIdQueryHandler(IReviewRepository reviews, ICurrentUser currentUser)
    {
        _reviews = reviews;
        _currentUser = currentUser;
    }

    public async Task<ReviewDetailDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdWithManuscriptAndReviewerAsync(request.Id, cancellationToken);

        if (review is null)
        {
            return null;
        }

        var canSee = _currentUser.HasPermission(Permissions.Reviews.ViewAll)
            || _currentUser.UserId == review.ReviewerId;

        if (!canSee)
        {
            throw new ForbiddenException("Bu değerlendirmeyi görme yetkiniz yok.");
        }

        var reviewerName = review.Reviewer is null
            ? string.Empty
            : AcademicTitles.FormatName(
                review.Reviewer.AcademicTitle,
                review.Reviewer.FirstName,
                review.Reviewer.LastName);

        return new ReviewDetailDto(
            review.Id,
            review.ManuscriptId,
            review.Manuscript?.Title ?? string.Empty,
            review.Manuscript?.Content ?? string.Empty,
            review.Manuscript?.Summary,
            review.ReviewerId,
            reviewerName,
            review.AssignedAtUtc,
            review.SubmittedAtUtc,
            new ReviewSummaryDto(
                review.Id,
                review.ReviewerId,
                reviewerName,
                review.AssignedAtUtc,
                review.SubmittedAtUtc,
                review.Recommendation,
                review.Comments));
    }
}
