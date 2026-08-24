using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using MediatR;

namespace Blog.Application.ResearchAreas.Queries.GetResearchAreaById;

public sealed record GetResearchAreaByIdQuery(int Id) : IRequest<ResearchAreaDto?>;

public sealed class GetResearchAreaByIdQueryHandler
    : IRequestHandler<GetResearchAreaByIdQuery, ResearchAreaDto?>
{
    private readonly IResearchAreaRepository _researchAreas;

    public GetResearchAreaByIdQueryHandler(IResearchAreaRepository researchAreas)
    {
        _researchAreas = researchAreas;
    }

    public async Task<ResearchAreaDto?> Handle(
        GetResearchAreaByIdQuery request,
        CancellationToken cancellationToken)
    {
        var row = await _researchAreas.GetWithManuscriptCountAsync(request.Id, cancellationToken);

        return row is null
            ? null
            : new ResearchAreaDto(row.Id, row.Name, row.Slug, row.ManuscriptCount);
    }
}
