using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.ResearchAreas.Queries.GetResearchAreaById;

public sealed record GetResearchAreaByIdQuery(int Id) : IRequest<ResearchAreaDto?>;

public sealed class GetResearchAreaByIdQueryHandler
    : IRequestHandler<GetResearchAreaByIdQuery, ResearchAreaDto?>
{
    private readonly IApplicationDbContext _db;

    public GetResearchAreaByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ResearchAreaDto?> Handle(
        GetResearchAreaByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.ResearchAreas
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new ResearchAreaDto(
                a.Id,
                a.Name,
                a.Slug,
                a.Manuscripts.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
