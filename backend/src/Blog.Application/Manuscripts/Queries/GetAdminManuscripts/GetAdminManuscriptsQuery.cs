using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using MediatR;

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
    private readonly IManuscriptRepository _manuscripts;
    private readonly ICurrentUser _currentUser;

    public GetAdminManuscriptsQueryHandler(
        IManuscriptRepository manuscripts,
        ICurrentUser currentUser)
    {
        _manuscripts = manuscripts;
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

        var includeReview = _currentUser.HasPermission(Permissions.Reviews.ViewAll);

        var (manuscripts, totalCount) = await _manuscripts.ListVisiblePagedAsync(
            page,
            pageSize,
            request.Search,
            request.ResearchAreaId,
            request.Status,
            _currentUser.UserId,
            ManuscriptAccess.CanViewAll(_currentUser),
            cancellationToken);

        var items = manuscripts.Select(m => new AdminManuscriptListItemDto(
            m.Id,
            m.Title,
            m.Slug,
            m.Summary,
            m.PublishedAt,
            m.Status,
            m.ResearchAreaId,
            m.ResearchArea?.Name ?? string.Empty,
            m.AuthorId,
            m.Author is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    m.Author.AcademicTitle,
                    m.Author.FirstName,
                    m.Author.LastName),
            includeReview ? MapCurrentReview(m) : null)).ToList();

        return new PagedResult<AdminManuscriptListItemDto>(items, page, pageSize, totalCount);
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
