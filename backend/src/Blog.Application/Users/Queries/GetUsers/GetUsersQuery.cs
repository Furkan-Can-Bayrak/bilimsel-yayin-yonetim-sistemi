using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Users.Dtos;
using MediatR;

namespace Blog.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<UserListItemDto>>;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<PagedResult<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var (users, totalCount) = await _users.ListPagedWithRolesAsync(
            page,
            pageSize,
            cancellationToken);

        var items = users.Select(u =>
        {
            var roles = u.UserRoles
                .Where(ur => ur.Role is not null)
                .OrderBy(ur => ur.Role!.Name)
                .ToList();

            return new UserListItemDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.AcademicTitle,
                u.IsActive,
                roles.Select(r => r.RoleId).ToList(),
                roles.Select(r => r.Role!.Name).ToList());
        }).ToList();

        return new PagedResult<UserListItemDto>(items, page, pageSize, totalCount);
    }
}
