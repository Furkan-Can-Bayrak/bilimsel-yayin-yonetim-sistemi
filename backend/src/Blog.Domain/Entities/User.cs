using Blog.Domain.Common;
using Blog.Domain.Enums;

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

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>Akademik unvan. Verilmezse Dr.</summary>
    public AcademicTitle AcademicTitle { get; set; } = AcademicTitle.Dr;

    /// <summary>Bağlı olduğu kurum. Bağımsız araştırmacıda null.</summary>
    public int? InstitutionId { get; set; }

    public Institution? Institution { get; set; }

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

    /// <summary>Unvansız görünen ad. Sorguda kullanma; EF bunu SQL'e çevirmez.</summary>
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    /// <summary>Unvanlı görünen ad. Sorguda kullanma; EF bunu SQL'e çevirmez.</summary>
    public string DisplayNameWithTitle => AcademicTitles.FormatName(AcademicTitle, FirstName, LastName);

    public void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("Ad zorunludur.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Soyad zorunludur.", nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}
