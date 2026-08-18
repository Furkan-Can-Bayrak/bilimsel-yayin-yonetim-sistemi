using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetReviewByIdQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReviewDetailDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new ReviewDetailDto(
                r.Id,
                r.ManuscriptId,
                r.Manuscript != null ? r.Manuscript.Title : string.Empty,
                r.Manuscript != null ? r.Manuscript.Content : string.Empty,
                r.Manuscript != null ? r.Manuscript.Summary : null,
                r.ReviewerId,
                r.Reviewer == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(r.Reviewer.AcademicTitle)
                        ? r.Reviewer.FirstName + " " + r.Reviewer.LastName
                        : r.Reviewer.AcademicTitle + " " + r.Reviewer.FirstName + " " + r.Reviewer.LastName,
                r.AssignedAtUtc,
                r.SubmittedAtUtc,
                new ReviewSummaryDto(
                    r.Id,
                    r.ReviewerId,
                    r.Reviewer == null ? string.Empty : r.Reviewer.FirstName + " " + r.Reviewer.LastName,
                    r.AssignedAtUtc,
                    r.SubmittedAtUtc,
                    r.Recommendation,
                    r.Comments)))
            .FirstOrDefaultAsync(cancellationToken);

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

        return review;
    }
}
