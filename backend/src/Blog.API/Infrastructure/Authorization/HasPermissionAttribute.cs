using Microsoft.AspNetCore.Authorization;

namespace Blog.API.Infrastructure.Authorization;

/// <summary>
/// Uç noktayı belirli bir izne bağlar, ör. <c>[HasPermission(Permissions.Manuscripts.Publish)]</c>.
/// İzin adını policy adına gömer; policy'yi <see cref="PermissionPolicyProvider"/> üretir,
/// böylece her izin için elle policy tanımlamak gerekmez.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PolicyPrefix}{permission}";
    }
}
