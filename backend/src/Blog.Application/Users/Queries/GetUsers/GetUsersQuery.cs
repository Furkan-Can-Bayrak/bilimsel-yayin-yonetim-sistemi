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
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.AcademicTitle,
                u.IsActive,
                u.UserRoles
                    .Select(ur => ur.Role!.Name)
                    .OrderBy(name => name)
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
