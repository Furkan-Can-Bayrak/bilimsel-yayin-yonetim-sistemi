using Blog.Application.Manuscripts.Commands.CreateManuscript;

namespace Blog.Application.Tests.Manuscripts;

public class CreateManuscriptCommandValidatorTests
{
    private readonly CreateManuscriptCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("İlk makale", "İçerik buraya", "Özet", 1, null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_title_fails()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("", "İçerik", null, 1, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateManuscriptCommand.Title));
    }

    [Fact]
    public void ResearchAreaId_must_be_greater_than_zero()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("Başlık", "İçerik", null, 0, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateManuscriptCommand.ResearchAreaId));
    }
}
