using Blog.Domain.Enums;

namespace Blog.Application.Users.Dtos;

public sealed record UserListItemDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    AcademicTitle AcademicTitle,
    bool IsActive,
    IReadOnlyList<int> RoleIds,
    IReadOnlyList<string> RoleNames);

public sealed record CreateUserResult(int Id, string Email);

public sealed record RoleListItemDto(int Id, string Name);
