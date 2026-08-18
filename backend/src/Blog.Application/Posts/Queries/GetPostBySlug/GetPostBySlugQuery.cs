using Blog.Application.Common.Interfaces;
using Blog.Application.Posts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Posts.Queries.GetPostBySlug;

/// <summary>
/// Slug ile tek yazı. Bulunamazsa null döner; controller 404 verir.
/// </summary>
public sealed record GetPostBySlugQuery(string Slug) : IRequest<PostDetailDto?>;

public sealed class GetPostBySlugQueryHandler
    : IRequestHandler<GetPostBySlugQuery, PostDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetPostBySlugQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PostDetailDto?> Handle(
        GetPostBySlugQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.IsPublished && p.Slug == request.Slug)
            .Select(p => new PostDetailDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.Summary,
                p.PublishedAt,
                p.Category != null ? p.Category.Name : string.Empty,
                p.Category != null ? p.Category.Slug : string.Empty))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
