using Microsoft.AspNetCore.Authorization;

namespace Blog.API.Infrastructure.Authorization;

/// <summary>
/// Tek bir izin kodunun sağlanmasını isteyen yetki kuralı.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
