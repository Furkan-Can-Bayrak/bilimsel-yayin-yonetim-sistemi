using Blog.Application.Common.Interfaces;
using Blog.Application.Posts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Posts.Queries.GetPostById;

/// <summary>Id ile yazı (taslak dahil) — admin düzenleme.</summary>
public sealed record GetPostByIdQuery(int Id) : IRequest<AdminPostDetailDto?>;

public sealed class GetPostByIdQueryHandler
    : IRequestHandler<GetPostByIdQuery, AdminPostDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetPostByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminPostDetailDto?> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new AdminPostDetailDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.Summary,
                p.PublishedAt,
                p.IsPublished,
                p.CategoryId,
                p.Category != null ? p.Category.Name : string.Empty))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
