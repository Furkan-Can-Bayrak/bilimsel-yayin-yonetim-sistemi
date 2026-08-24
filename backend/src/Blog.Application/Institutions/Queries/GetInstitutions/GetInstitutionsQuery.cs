using Blog.Application.Common.Interfaces;
using Blog.Application.Institutions.Dtos;
using MediatR;

namespace Blog.Application.Institutions.Queries.GetInstitutions;

public sealed record GetInstitutionsQuery : IRequest<IReadOnlyList<InstitutionListItemDto>>;

public sealed class GetInstitutionsQueryHandler
    : IRequestHandler<GetInstitutionsQuery, IReadOnlyList<InstitutionListItemDto>>
{
    private readonly IInstitutionRepository _institutions;

    public GetInstitutionsQueryHandler(IInstitutionRepository institutions)
    {
        _institutions = institutions;
    }

    public async Task<IReadOnlyList<InstitutionListItemDto>> Handle(
        GetInstitutionsQuery request,
        CancellationToken cancellationToken)
    {
        var institutions = await _institutions.ListOrderedByNameAsync(cancellationToken);

        return institutions
            .Select(i => new InstitutionListItemDto(i.Id, i.Name, i.EmailDomain))
            .ToList();
    }
}
