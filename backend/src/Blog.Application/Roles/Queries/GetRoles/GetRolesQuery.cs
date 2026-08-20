using Blog.Application.Common.Interfaces;
using Blog.Application.Users.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleListItemDto>>;

public sealed class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetRolesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RoleListItemDto>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleListItemDto(r.Id, r.Name))
            .ToListAsync(cancellationToken);
    }
}
