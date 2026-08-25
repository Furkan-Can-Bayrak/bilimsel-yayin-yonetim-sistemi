using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptById;

public sealed record GetManuscriptByIdQuery(int Id) : IRequest<AdminManuscriptDetailDto?>;

public sealed class GetManuscriptByIdQueryHandler
    : IRequestHandler<GetManuscriptByIdQuery, AdminManuscriptDetailDto?>
{
    private readonly IManuscriptRepository _manuscripts;
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;

    public GetManuscriptByIdQueryHandler(
        IManuscriptRepository manuscripts,
        IReviewRepository reviews,
        ICurrentUser currentUser)
    {
        _manuscripts = manuscripts;
        _reviews = reviews;
        _currentUser = currentUser;
    }

    public async Task<AdminManuscriptDetailDto?> Handle(
        GetManuscriptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var includeReview = _currentUser.HasPermission(Permissions.Reviews.ViewAll)
            || _currentUser.HasPermission(Permissions.Reviews.Submit);

        var manuscript = await _manuscripts.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            return null;
        }

        var isAssignedReviewer = _currentUser.UserId is int userId &&
            await _reviews.ExistsForManuscriptAndReviewerAsync(
                request.Id,
                userId,
                cancellationToken);

        if (!ManuscriptAccess.CanViewRecord(manuscript, _currentUser, isAssignedReviewer))
        {
            return null;
        }

        var dto = Map(manuscript, includeReview);

        if (!ManuscriptAccess.CanViewAll(_currentUser)
            && dto.CurrentReview is not null
            && dto.CurrentReview.ReviewerId != _currentUser.UserId)
        {
            return dto with { CurrentReview = null };
        }

        if (!ManuscriptAccess.CanViewAll(_currentUser)
            && !_currentUser.HasPermission(Permissions.Reviews.ViewAll)
            && _currentUser.UserId == manuscript.AuthorId)
        {
            return dto with { CurrentReview = null };
        }

        return dto;
    }

    private static AdminManuscriptDetailDto Map(Manuscript manuscript, bool includeReview)
    {
        return new AdminManuscriptDetailDto(
            manuscript.Id,
            manuscript.Title,
            manuscript.Slug,
            manuscript.Content,
            manuscript.Summary,
            manuscript.PublishedAt,
            manuscript.Status,
            manuscript.ResearchAreaId,
            manuscript.ResearchArea?.Name ?? string.Empty,
            manuscript.AuthorId,
            manuscript.Author is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    manuscript.Author.AcademicTitle,
                    manuscript.Author.FirstName,
                    manuscript.Author.LastName),
            includeReview ? MapCurrentReview(manuscript) : null);
    }

    private static ReviewSummaryDto? MapCurrentReview(Manuscript manuscript)
    {
        var review = manuscript.Reviews
            .OrderByDescending(r => r.AssignedAtUtc)
            .FirstOrDefault();

        if (review is null)
        {
            return null;
        }

        return new ReviewSummaryDto(
            review.Id,
            review.ReviewerId,
            review.Reviewer is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    review.Reviewer.AcademicTitle,
                    review.Reviewer.FirstName,
                    review.Reviewer.LastName),
            review.AssignedAtUtc,
            review.SubmittedAtUtc,
            review.Recommendation,
            review.Comments);
    }
}
