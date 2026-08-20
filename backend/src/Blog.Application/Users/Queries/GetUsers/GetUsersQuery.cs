using Blog.Application.Common.Interfaces;
using Blog.Application.Users.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUsersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.AcademicTitle,
                u.IsActive,
                Roles = u.UserRoles
                    .OrderBy(ur => ur.Role!.Name)
                    .Select(ur => new { ur.RoleId, Name = ur.Role!.Name })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(u => new UserListItemDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.AcademicTitle,
            u.IsActive,
            u.Roles.ConvertAll(r => r.RoleId),
            u.Roles.ConvertAll(r => r.Name)));
    }
}
