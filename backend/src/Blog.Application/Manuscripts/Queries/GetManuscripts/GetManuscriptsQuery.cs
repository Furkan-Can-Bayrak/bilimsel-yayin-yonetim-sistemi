using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetManuscripts;

public sealed record GetManuscriptsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? ResearchAreaId = null) : IRequest<PagedResult<ManuscriptListItemDto>>;

public sealed class GetManuscriptsQueryHandler
    : IRequestHandler<GetManuscriptsQuery, PagedResult<ManuscriptListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IApplicationDbContext _db;

    public GetManuscriptsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ManuscriptListItemDto>> Handle(
        GetManuscriptsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var query = _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.IsPublished);

        if (request.ResearchAreaId is int researchAreaId)
        {
            query = query.Where(m => m.ResearchAreaId == researchAreaId);
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
            .OrderByDescending(m => m.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ManuscriptListItemDto(
                m.Id,
                m.Title,
                m.Slug,
                m.Summary,
                m.PublishedAt,
                m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.Author == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(m.Author.AcademicTitle)
                        ? m.Author.FullName
                        : m.Author.AcademicTitle + " " + m.Author.FullName))
            .ToListAsync(cancellationToken);

        return new PagedResult<ManuscriptListItemDto>(items, page, pageSize, totalCount);
    }
}
