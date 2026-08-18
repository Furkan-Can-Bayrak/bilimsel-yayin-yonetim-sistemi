using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetAdminManuscripts;

public sealed record GetAdminManuscriptsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? ResearchAreaId = null,
    bool? IsPublished = null) : IRequest<PagedResult<AdminManuscriptListItemDto>>;

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

        if (request.IsPublished is bool isPublished)
        {
            query = query.Where(m => m.IsPublished == isPublished);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(m =>
                m.Title.Contains(term) ||
                (m.Summary != null && m.Summary.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new AdminManuscriptListItemDto(
                m.Id,
                m.Title,
                m.Slug,
                m.Summary,
                m.PublishedAt,
                m.IsPublished,
                m.ResearchAreaId,
                m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.AuthorId,
                m.Author == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(m.Author.AcademicTitle)
                        ? m.Author.FullName
                        : m.Author.AcademicTitle + " " + m.Author.FullName))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminManuscriptListItemDto>(items, page, pageSize, totalCount);
    }
}
