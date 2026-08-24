using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithRolesAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> OrcidExistsAsync(
        string orcid,
        CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(
        int userId,
        string permissionCode,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<User> Items, int TotalCount)> ListPagedWithRolesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListActiveByPermissionAsync(
        string permissionCode,
        int excludeUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ListActiveIdsByPermissionAsync(
        string permissionCode,
        int? excludeUserId,
        CancellationToken cancellationToken = default);
}
