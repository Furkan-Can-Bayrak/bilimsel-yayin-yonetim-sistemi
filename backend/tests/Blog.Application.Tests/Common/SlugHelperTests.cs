using Blog.Application.Common;
using Blog.Application.Common.Exceptions;

namespace Blog.Application.Tests.Common;

public class SlugHelperTests
{
    [Fact]
    public void GenerateSlug_builds_ascii_slug()
    {
        var slug = SlugHelper.GenerateSlug("Derin Öğrenme ile Makale");

        Assert.Equal("derin-ogrenme-ile-makale", slug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateSlug_rejects_blank_source(string? value)
    {
        Assert.Throws<ArgumentException>(() => SlugHelper.GenerateSlug(value!));
    }

    [Fact]
    public void GenerateSlug_rejects_unusable_source()
    {
        var ex = Assert.Throws<AppValidationException>(() => SlugHelper.GenerateSlug("???"));

        Assert.Equal(["Bu metinden URL üretilemedi."], ex.Errors["Slug"]);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_appends_suffix_when_taken()
    {
        var taken = new HashSet<string> { "derin-ogrenme" };

        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            "Derin Öğrenme",
            "Title",
            s => Task.FromResult(taken.Contains(s)));

        Assert.Equal("derin-ogrenme-2", slug);
    }
}
