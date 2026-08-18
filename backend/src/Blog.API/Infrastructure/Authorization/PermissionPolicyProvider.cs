using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Blog.API.Infrastructure.Authorization;

/// <summary>
/// "Permission:" ile başlayan policy adlarını çalışma zamanında üretir. İzinler koda
/// eklendikçe <c>AddAuthorization</c> içinde tek tek policy tanımlamak gerekmez.
/// Tanımadığı adları varsayılan sağlayıcıya devreder.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new(StringComparer.Ordinal);

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
        {
            return _fallbackProvider.GetPolicyAsync(policyName);
        }

        // Policy her istekte sorulur; aynı izin için nesneyi yeniden kurmamak adına saklıyoruz.
        var policy = _cache.GetOrAdd(policyName, static name =>
        {
            var permission = name[HasPermissionAttribute.PolicyPrefix.Length..];

            return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        });

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
