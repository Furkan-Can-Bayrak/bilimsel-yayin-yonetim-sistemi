using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
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
        var row = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new
            {
                r.Id,
                r.ManuscriptId,
                ManuscriptTitle = r.Manuscript != null ? r.Manuscript.Title : string.Empty,
                ManuscriptContent = r.Manuscript != null ? r.Manuscript.Content : string.Empty,
                ManuscriptSummary = r.Manuscript != null ? r.Manuscript.Summary : null,
                r.ReviewerId,
                ReviewerTitle = r.Reviewer == null ? AcademicTitle.Dr : r.Reviewer.AcademicTitle,
                ReviewerFirstName = r.Reviewer == null ? string.Empty : r.Reviewer.FirstName,
                ReviewerLastName = r.Reviewer == null ? string.Empty : r.Reviewer.LastName,
                r.AssignedAtUtc,
                r.SubmittedAtUtc,
                r.Recommendation,
                r.Comments
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var reviewerName = AcademicTitles.FormatName(
            row.ReviewerTitle,
            row.ReviewerFirstName,
            row.ReviewerLastName);

        var review = new ReviewDetailDto(
            row.Id,
            row.ManuscriptId,
            row.ManuscriptTitle,
            row.ManuscriptContent,
            row.ManuscriptSummary,
            row.ReviewerId,
            reviewerName,
            row.AssignedAtUtc,
            row.SubmittedAtUtc,
            new ReviewSummaryDto(
                row.Id,
                row.ReviewerId,
                reviewerName,
                row.AssignedAtUtc,
                row.SubmittedAtUtc,
                row.Recommendation,
                row.Comments));

        var canSee = _currentUser.HasPermission(Permissions.Reviews.ViewAll)
            || _currentUser.UserId == review.ReviewerId;

        if (!canSee)
        {
            throw new ForbiddenException("Bu değerlendirmeyi görme yetkiniz yok.");
        }

        return review;
    }
}
