namespace Blog.Application.Common.Authorization;

/// <summary>
/// Token'a yazılan özel claim adları. Token'ı üreten Infrastructure ile
/// okuyan API'nin aynı anahtarları kullanması için tek kaynak.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Kullanıcının sahip olduğu her izin için bir tane yazılır.</summary>
    public const string Permission = "permission";

    /// <summary>Token üretilirken kullanıcının <c>SecurityVersion</c> değeri.</summary>
    public const string SecurityVersion = "security_version";

    public const string FullName = "full_name";
}
