using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using MediatR;

namespace Blog.Application.ResearchAreas.Queries.GetResearchAreas;

public sealed record GetResearchAreasQuery : IRequest<IReadOnlyList<ResearchAreaDto>>;

public sealed class GetResearchAreasQueryHandler
    : IRequestHandler<GetResearchAreasQuery, IReadOnlyList<ResearchAreaDto>>
{
    private readonly IResearchAreaRepository _researchAreas;

    public GetResearchAreasQueryHandler(IResearchAreaRepository researchAreas)
    {
        _researchAreas = researchAreas;
    }

    public Task<IReadOnlyList<ResearchAreaDto>> Handle(
        GetResearchAreasQuery request,
        CancellationToken cancellationToken) =>
        _researchAreas.ListWithManuscriptCountsAsync(cancellationToken);
}
