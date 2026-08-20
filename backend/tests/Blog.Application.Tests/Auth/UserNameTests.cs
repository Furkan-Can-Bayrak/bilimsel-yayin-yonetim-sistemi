using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Tests.Auth;

public class UserNameTests
{
    [Fact]
    public void SetName_stores_trimmed_parts_and_uppercases_last_name()
    {
        var user = new User();

        user.SetName("  Elif ", " Demir ");

        Assert.Equal("Elif", user.FirstName);
        Assert.Equal("DEMİR", user.LastName);
        Assert.Equal("Elif DEMİR", user.DisplayName);
        Assert.Equal("Dr. Elif DEMİR", user.DisplayNameWithTitle);
    }

    [Fact]
    public void DisplayNameWithTitle_prefixes_label()
    {
        var user = new User { AcademicTitle = AcademicTitle.DrOgrUyesi };

        user.SetName("Elif", "Demir");

        Assert.Equal("Dr. Öğr. Üyesi Elif DEMİR", user.DisplayNameWithTitle);
    }

    [Fact]
    public void SetName_requires_first_and_last()
    {
        var user = new User();

        Assert.Throws<ArgumentException>(() => user.SetName(" ", "Demir"));
        Assert.Throws<ArgumentException>(() => user.SetName("Elif", "\t"));
    }

    [Fact]
    public void SetName_uppercases_turkish_dotless_i()
    {
        var user = new User();

        user.SetName("Ayşe", "Yılmaz");

        Assert.Equal("YILMAZ", user.LastName);
    }
}

public class AcademicTitleTests
{
    [Theory]
    [InlineData(AcademicTitle.ProfDr, "Prof. Dr.")]
    [InlineData(AcademicTitle.DocDr, "Doç. Dr.")]
    [InlineData(AcademicTitle.DrOgrUyesi, "Dr. Öğr. Üyesi")]
    [InlineData(AcademicTitle.OgrGor, "Öğr. Gör.")]
    [InlineData(AcademicTitle.ArsGor, "Arş. Gör.")]
    [InlineData(AcademicTitle.Dr, "Dr.")]
    public void ToLabel_returns_display_text(AcademicTitle title, string expected)
    {
        Assert.Equal(expected, title.ToLabel());
    }

    [Fact]
    public void FormatName_uses_dr_when_title_is_unknown()
    {
        Assert.Equal("Dr. Elif Demir", AcademicTitles.FormatName((AcademicTitle)0, "Elif", "Demir"));
    }
}
