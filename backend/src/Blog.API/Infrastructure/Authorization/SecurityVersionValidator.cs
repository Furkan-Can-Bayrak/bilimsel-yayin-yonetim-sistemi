using System.Security.Claims;
using Blog.Application.Common.Authorization;
using Blog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Blog.API.Infrastructure.Authorization;

/// <summary>
/// Token imzası geçerli olsa bile hesabın hâlâ o yetkilere sahip olduğunu doğrular.
/// İzinler token'ın içinde taşındığı için, yetkisi değişen kullanıcı bu kontrol olmadan
/// token'ın ömrü boyunca eski yetkilerini kullanmaya devam ederdi.
/// </summary>
public static class SecurityVersionValidator
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        // Varsayılan inbound claim eşlemesi standart "sub" iddiasını NameIdentifier'a çevirir.
        var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var versionClaim = context.Principal?.FindFirstValue(AppClaimTypes.SecurityVersion);

        if (!int.TryParse(userIdClaim, out var userId) ||
            !int.TryParse(versionClaim, out var tokenVersion))
        {
            context.Fail("Token beklenen kimlik bilgilerini taşımıyor.");
            return;
        }

        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

        // Silinmiş kullanıcılar query filter ile düştüğü için burada da bulunamaz;
        // yani hesabın silinmesi tokenlarını anında geçersiz kılar.
        var account = await users.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        if (account is null || !account.IsActive)
        {
            context.Fail("Hesap artık erişilebilir değil.");
            return;
        }

        if (account.SecurityVersion != tokenVersion)
        {
            context.Fail("Yetkiler değişti, lütfen tekrar giriş yapın.");
        }
    }
}
