using Blog.Application.Common.Interfaces;
using Blog.Application.Institutions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Institutions.Queries.GetInstitutions;

public sealed record GetInstitutionsQuery : IRequest<IReadOnlyList<InstitutionListItemDto>>;

public sealed class GetInstitutionsQueryHandler
    : IRequestHandler<GetInstitutionsQuery, IReadOnlyList<InstitutionListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetInstitutionsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<InstitutionListItemDto>> Handle(
        GetInstitutionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Institutions
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new InstitutionListItemDto(i.Id, i.Name, i.EmailDomain))
            .ToListAsync(cancellationToken);
    }
}
