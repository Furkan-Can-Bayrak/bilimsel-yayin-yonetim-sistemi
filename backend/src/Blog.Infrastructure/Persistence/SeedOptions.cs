namespace Blog.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>Yönetici hesabının e-postası. Verilmezse varsayılan kullanılır.</summary>
    public string AdminEmail { get; set; } = "admin@yayin.local";

    /// <summary>Zorunlu. User Secrets veya Seed__AdminPassword ortam değişkeni.</summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Development demo hesaplarının şifresi. Verilmezse ilk oluşturmada AdminPassword kullanılır.
    /// Verildiyse her açılışta yönetici dahil dört seed hesabı bu şifreye eşitlenir.
    /// </summary>
    public string DemoPassword { get; set; } = string.Empty;
}
