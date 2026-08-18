using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

/// <summary>Üretilen token ve geçerlilik bitişi.</summary>
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    /// <summary>
    /// Kullanıcı için token üretir. Rol ve izinler dışarıdan verilir; böylece servis
    /// navigasyonların yüklenmiş olmasına bel bağlamaz.
    /// </summary>
    AccessToken CreateToken(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
}
