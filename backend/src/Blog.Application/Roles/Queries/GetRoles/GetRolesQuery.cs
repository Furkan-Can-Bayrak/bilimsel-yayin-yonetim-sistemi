using Blog.Application.Common.Interfaces;
using Blog.Application.Users.Dtos;
using MediatR;

namespace Blog.Application.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleListItemDto>>;

public sealed class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    private readonly IRoleRepository _roles;

    public GetRolesQueryHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public async Task<IReadOnlyList<RoleListItemDto>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _roles.ListOrderedByNameAsync(cancellationToken);

        return roles
            .Select(r => new RoleListItemDto(r.Id, r.Name))
            .ToList();
    }
}
