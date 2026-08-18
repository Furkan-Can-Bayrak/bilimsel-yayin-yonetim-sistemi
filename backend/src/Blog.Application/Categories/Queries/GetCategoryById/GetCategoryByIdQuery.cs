using Blog.Application.Categories.Dtos;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto?>;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IApplicationDbContext _db;

    public GetCategoryByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Posts.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
