namespace Blog.Domain.Enums;

/// <summary>
/// Türkiye'de yaygın akademik unvanlar. Numaralar sabittir; yeni unvan sona eklenir.
/// Verilmemiş unvan Dr kabul edilir.
/// </summary>
public enum AcademicTitle
{
    ProfDr = 1,
    DocDr = 2,
    DrOgrUyesi = 3,
    OgrGor = 4,
    ArsGor = 5,
    Dr = 6
}

public static class AcademicTitles
{
    public static string ToLabel(this AcademicTitle title) => title switch
    {
        AcademicTitle.ProfDr => "Prof. Dr.",
        AcademicTitle.DocDr => "Doç. Dr.",
        AcademicTitle.DrOgrUyesi => "Dr. Öğr. Üyesi",
        AcademicTitle.OgrGor => "Öğr. Gör.",
        AcademicTitle.ArsGor => "Arş. Gör.",
        AcademicTitle.Dr => "Dr.",
        _ => "Dr."
    };

    public static string FormatName(AcademicTitle title, string firstName, string lastName)
    {
        var name = $"{firstName} {lastName}".Trim();
        return string.IsNullOrEmpty(name) ? string.Empty : $"{title.ToLabel()} {name}";
    }
}
