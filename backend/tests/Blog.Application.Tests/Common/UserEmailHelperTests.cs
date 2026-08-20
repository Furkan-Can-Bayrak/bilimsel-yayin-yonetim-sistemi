using Blog.Application.Common;

namespace Blog.Application.Tests.Common;

public class UserEmailHelperTests
{
    [Fact]
    public void BuildLocalPart_mustafa_ulas()
    {
        Assert.Equal("mulas", UserEmailHelper.BuildLocalPart("Mustafa", "Ulaş"));
    }

    [Fact]
    public void BuildLocalPart_elif_bahar_ozdogru()
    {
        Assert.Equal("ebozdogru", UserEmailHelper.BuildLocalPart("Elif Bahar", "Özdoğru"));
    }

    [Fact]
    public async Task BuildUniqueEmailAsync_adds_suffix_when_taken()
    {
        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mulas@firat.edu.tr"
        };

        var email = await UserEmailHelper.BuildUniqueEmailAsync(
            "Mustafa",
            "Ulaş",
            "firat.edu.tr",
            candidate => Task.FromResult(taken.Contains(candidate)));

        Assert.Equal("mulas2@firat.edu.tr", email);
    }

    [Fact]
    public void GeneratePassword_has_expected_length()
    {
        var password = UserEmailHelper.GeneratePassword(12);
        Assert.Equal(12, password.Length);
    }
}
