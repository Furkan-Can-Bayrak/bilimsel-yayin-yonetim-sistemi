using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetAdminManuscripts;

public sealed record GetAdminManuscriptsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? ResearchAreaId = null,
    ManuscriptStatus? Status = null) : IRequest<PagedResult<AdminManuscriptListItemDto>>;

public sealed class GetAdminManuscriptsQueryHandler
    : IRequestHandler<GetAdminManuscriptsQuery, PagedResult<AdminManuscriptListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAdminManuscriptsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AdminManuscriptListItemDto>> Handle(
        GetAdminManuscriptsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var query = ManuscriptAccess.VisibleTo(_db.Manuscripts.AsNoTracking(), _currentUser);

        if (request.ResearchAreaId is int researchAreaId)
        {
            query = query.Where(m => m.ResearchAreaId == researchAreaId);
        }

        if (request.Status is ManuscriptStatus status)
        {
            query = query.Where(m => m.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(m =>
                m.Title.Contains(term) ||
                (m.Summary != null && m.Summary.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var includeReview = _currentUser.HasPermission(Permissions.Reviews.ViewAll);

        var rows = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.Slug,
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
            .ToListAsync(cancellationToken);

        var items = rows.ConvertAll(m => new AdminManuscriptListItemDto(
            m.Id,
            m.Title,
            m.Slug,
            m.Summary,
            m.PublishedAt,
            m.Status,
            m.ResearchAreaId,
            m.ResearchAreaName,
            m.AuthorId,
            AcademicTitles.FormatName(m.AuthorTitle, m.AuthorFirstName, m.AuthorLastName),
            m.CurrentReview is null
                ? null
                : new ReviewSummaryDto(
                    m.CurrentReview.Id,
                    m.CurrentReview.ReviewerId,
                    AcademicTitles.FormatName(
                        m.CurrentReview.ReviewerTitle,
                        m.CurrentReview.ReviewerFirstName,
                        m.CurrentReview.ReviewerLastName),
                    m.CurrentReview.AssignedAtUtc,
                    m.CurrentReview.SubmittedAtUtc,
                    m.CurrentReview.Recommendation,
                    m.CurrentReview.Comments)));

        return new PagedResult<AdminManuscriptListItemDto>(items, page, pageSize, totalCount);
    }
}
