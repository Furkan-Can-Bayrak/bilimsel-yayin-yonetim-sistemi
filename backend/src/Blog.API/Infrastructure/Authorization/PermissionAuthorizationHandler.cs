using Blog.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Blog.API.Infrastructure.Authorization;

/// <summary>
/// İzin kararını token'daki claim'lere bakarak verir; veritabanına gitmez.
/// Rol veya izin değişikliklerinin anında etkili olmasını
/// <see cref="SecurityVersionValidator"/> üstlenir.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(AppClaimTypes.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
