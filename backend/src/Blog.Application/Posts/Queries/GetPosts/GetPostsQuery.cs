using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Posts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Posts.Queries.GetPosts;

/// <summary>
/// Public yazı listesi — sadece yayınlanmış.
/// Filtre: search (title/summary), categoryId. Sayfalama: page / pageSize.
/// </summary>
public sealed record GetPostsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? CategoryId = null) : IRequest<PagedResult<PostListItemDto>>;

public sealed class GetPostsQueryHandler
    : IRequestHandler<GetPostsQuery, PagedResult<PostListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IApplicationDbContext _db;

    public GetPostsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PostListItemDto>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var query = _db.Posts
            .AsNoTracking()
            .Where(p => p.IsPublished);

        if (request.CategoryId is int categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
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
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Summary,
                p.PublishedAt,
                p.Category != null ? p.Category.Name : string.Empty))
            .ToListAsync(cancellationToken);

        return new PagedResult<PostListItemDto>(items, page, pageSize, totalCount);
    }
}
