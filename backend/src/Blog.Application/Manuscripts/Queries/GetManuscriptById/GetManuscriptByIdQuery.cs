using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptById;

public sealed record GetManuscriptByIdQuery(int Id) : IRequest<AdminManuscriptDetailDto?>;

public sealed class GetManuscriptByIdQueryHandler
    : IRequestHandler<GetManuscriptByIdQuery, AdminManuscriptDetailDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetManuscriptByIdQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AdminManuscriptDetailDto?> Handle(
        GetManuscriptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var includeReview = _currentUser.HasPermission(Permissions.Reviews.ViewAll)
            || _currentUser.HasPermission(Permissions.Reviews.Submit);

        var row = await _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.Slug,
                m.Content,
                m.Summary,
                m.PublishedAt,
                m.Status,
                m.ResearchAreaId,
                ResearchAreaName = m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.AuthorId,
                AuthorTitle = m.Author == null ? AcademicTitle.Dr : m.Author.AcademicTitle,
                AuthorFirstName = m.Author == null ? string.Empty : m.Author.FirstName,
                AuthorLastName = m.Author == null ? string.Empty : m.Author.LastName,
                CurrentReview = includeReview
                    ? m.Reviews
                        .OrderByDescending(r => r.AssignedAtUtc)
                        .Select(r => new
                        {
                            r.Id,
                            r.ReviewerId,
                            ReviewerTitle = r.Reviewer == null ? AcademicTitle.Dr : r.Reviewer.AcademicTitle,
                            ReviewerFirstName = r.Reviewer == null ? string.Empty : r.Reviewer.FirstName,
                            ReviewerLastName = r.Reviewer == null ? string.Empty : r.Reviewer.LastName,
                            r.AssignedAtUtc,
                            r.SubmittedAtUtc,
                            r.Recommendation,
                            r.Comments
                        })
                        .FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var manuscript = new AdminManuscriptDetailDto(
            row.Id,
            row.Title,
            row.Slug,
            row.Content,
            row.Summary,
            row.PublishedAt,
            row.Status,
            row.ResearchAreaId,
            row.ResearchAreaName,
            row.AuthorId,
            AcademicTitles.FormatName(row.AuthorTitle, row.AuthorFirstName, row.AuthorLastName),
            row.CurrentReview is null
                ? null
                : new ReviewSummaryDto(
                    row.CurrentReview.Id,
                    row.CurrentReview.ReviewerId,
                    AcademicTitles.FormatName(
                        row.CurrentReview.ReviewerTitle,
                        row.CurrentReview.ReviewerFirstName,
                        row.CurrentReview.ReviewerLastName),
                    row.CurrentReview.AssignedAtUtc,
                    row.CurrentReview.SubmittedAtUtc,
                    row.CurrentReview.Recommendation,
                    row.CurrentReview.Comments));

        var isAssignedReviewer = _currentUser.UserId is int userId &&
            await _db.Reviews.AnyAsync(
                r => r.ManuscriptId == request.Id && r.ReviewerId == userId,
                cancellationToken);

        if (!ManuscriptAccess.CanView(manuscript.AuthorId, _currentUser, isAssignedReviewer))
        {
            return null;
        }

        // Yazar raporu görmesin; hakem yalnızca kendi atamasını görsün.
        if (!ManuscriptAccess.CanViewAll(_currentUser)
            && manuscript.CurrentReview is not null
            && manuscript.CurrentReview.ReviewerId != _currentUser.UserId)
        {
            return manuscript with { CurrentReview = null };
        }

        if (!ManuscriptAccess.CanViewAll(_currentUser)
            && !_currentUser.HasPermission(Permissions.Reviews.ViewAll)
            && _currentUser.UserId == manuscript.AuthorId)
        {
            return manuscript with { CurrentReview = null };
        }

        return manuscript;
    }
}
