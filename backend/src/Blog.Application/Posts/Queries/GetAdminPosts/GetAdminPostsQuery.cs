using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Posts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Posts.Queries.GetAdminPosts;

/// <summary>
/// Admin yazı listesi — taslak + yayında.
/// Filtre: search, categoryId, isPublished. Sayfalama: page / pageSize.
/// </summary>
public sealed record GetAdminPostsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? CategoryId = null,
    bool? IsPublished = null) : IRequest<PagedResult<AdminPostListItemDto>>;

public sealed class GetAdminPostsQueryHandler
    : IRequestHandler<GetAdminPostsQuery, PagedResult<AdminPostListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IApplicationDbContext _db;

    public GetAdminPostsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminPostListItemDto>> Handle(
        GetAdminPostsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var query = _db.Posts.AsNoTracking();

        if (request.CategoryId is int categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (request.IsPublished is bool isPublished)
        {
            query = query.Where(p => p.IsPublished == isPublished);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                (p.Summary != null && p.Summary.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPostListItemDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Summary,
                p.PublishedAt,
                p.IsPublished,
                p.CategoryId,
                p.Category != null ? p.Category.Name : string.Empty))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPostListItemDto>(items, page, pageSize, totalCount);
    }
}
