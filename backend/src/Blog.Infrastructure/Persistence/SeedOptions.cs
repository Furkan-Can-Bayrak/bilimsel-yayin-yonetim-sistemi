namespace Blog.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>Yönetici hesabının e-postası. Verilmezse fcbayrak@firat.edu.tr.</summary>
    public string AdminEmail { get; set; } = "fcbayrak@firat.edu.tr";

    /// <summary>Zorunlu. User Secrets veya Seed__AdminPassword ortam değişkeni.</summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Development'ta verildiyse her açılışta yönetici şifresi buna eşitlenir.
    /// Verilmezse yalnızca ilk oluşturmada AdminPassword kullanılır.
    /// </summary>
    public string DemoPassword { get; set; } = string.Empty;
}
