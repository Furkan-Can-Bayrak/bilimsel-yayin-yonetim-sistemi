using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Users.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<UserListItemDto>>;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IApplicationDbContext _db;

    public GetUsersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var query = _db.Users.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        var items = rows.ConvertAll(u => new UserListItemDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.AcademicTitle,
            u.IsActive,
            u.Roles.ConvertAll(r => r.RoleId),
            u.Roles.ConvertAll(r => r.Name)));

        return new PagedResult<UserListItemDto>(items, page, pageSize, totalCount);
    }
}
