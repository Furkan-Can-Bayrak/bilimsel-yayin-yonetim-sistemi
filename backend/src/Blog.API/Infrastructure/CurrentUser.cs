using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blog.Application.Common.Authorization;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;

namespace Blog.API.Infrastructure;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var raw = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool HasPermission(string permission)
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return false;
        }

        return principal.Claims.Any(claim =>
            claim.Type == AppClaimTypes.Permission && claim.Value == permission);
    }

    public int RequireUserId()
    {
        return UserId ?? throw new UnauthorizedException("Oturum bilgisi okunamadı.");
    }
}
