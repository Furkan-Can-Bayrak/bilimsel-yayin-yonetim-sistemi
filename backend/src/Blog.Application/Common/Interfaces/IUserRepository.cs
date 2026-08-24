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
}
