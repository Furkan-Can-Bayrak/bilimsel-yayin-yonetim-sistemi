using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.ResearchAreas.Queries.GetResearchAreas;

public sealed record GetResearchAreasQuery : IRequest<IReadOnlyList<ResearchAreaDto>>;

public sealed class GetResearchAreasQueryHandler
    : IRequestHandler<GetResearchAreasQuery, IReadOnlyList<ResearchAreaDto>>
{
    private readonly IApplicationDbContext _db;

    public GetResearchAreasQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ResearchAreaDto>> Handle(
        GetResearchAreasQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.ResearchAreas
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ResearchAreaDto(
                a.Id,
                a.Name,
                a.Slug,
                a.Manuscripts.Count))
            .ToListAsync(cancellationToken);
    }
}
