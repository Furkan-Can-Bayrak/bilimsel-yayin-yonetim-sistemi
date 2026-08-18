using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
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

        var manuscript = await _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new AdminManuscriptDetailDto(
                m.Id,
                m.Title,
                m.Slug,
                m.Content,
                m.Summary,
                m.PublishedAt,
                m.Status,
                m.ResearchAreaId,
                m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.AuthorId,
                m.Author == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(m.Author.AcademicTitle)
                        ? m.Author.FirstName + " " + m.Author.LastName
                        : m.Author.AcademicTitle + " " + m.Author.FirstName + " " + m.Author.LastName,
                includeReview
                    ? m.Reviews
                        .OrderByDescending(r => r.AssignedAtUtc)
                        .Select(r => new ReviewSummaryDto(
                            r.Id,
                            r.ReviewerId,
                            r.Reviewer == null
                                ? string.Empty
                                : string.IsNullOrWhiteSpace(r.Reviewer.AcademicTitle)
                                    ? r.Reviewer.FirstName + " " + r.Reviewer.LastName
                                    : r.Reviewer.AcademicTitle + " " + r.Reviewer.FirstName + " " + r.Reviewer.LastName,
                            r.AssignedAtUtc,
                            r.SubmittedAtUtc,
                            r.Recommendation,
                            r.Comments))
                        .FirstOrDefault()
                    : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (manuscript is null)
        {
            return null;
        }

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
