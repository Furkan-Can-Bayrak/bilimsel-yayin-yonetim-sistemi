using Blog.Domain.Common;

namespace Blog.Domain.Entities;

/// <summary>
/// Sistem kullanıcısı — yazar, hakem, editör. Yetkileri rolleri üzerinden gelir.
/// </summary>
public sealed class User : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>Giriş kimliği ve bildirim adresi.</summary>
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Makalelerde yazar olarak görünen ad.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Akademik unvan, ör. "Prof. Dr.", "Dr. Öğr. Üyesi".</summary>
    public string? AcademicTitle { get; set; }

    /// <summary>Bağlı olduğu kurum.</summary>
    public string? Affiliation { get; set; }

    /// <summary>Araştırmacı kimlik numarası, ör. "0000-0002-1825-0097".</summary>
    public string? Orcid { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Rolleri veya izinleri değiştiğinde artar. Token içindeki değer bununla uyuşmazsa
    /// istek reddedilir; böylece yetki değişikliği token süresi beklenmeden etkili olur.
    /// </summary>
    public int SecurityVersion { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
