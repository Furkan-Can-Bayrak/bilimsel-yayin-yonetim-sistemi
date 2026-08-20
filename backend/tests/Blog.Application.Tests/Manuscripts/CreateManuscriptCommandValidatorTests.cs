using Blog.Application.Manuscripts.Commands.CreateManuscript;

namespace Blog.Application.Tests.Manuscripts;

public class CreateManuscriptCommandValidatorTests
{
    private readonly CreateManuscriptCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("İlk makale", "İçerik buraya", "Özet", 1));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_draft_fails()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("", "", null, null, SubmitForReview: false));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Draft_with_only_summary_passes()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("", "", "Kısa özet", null, SubmitForReview: false));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Submit_requires_title_content_and_area()
    {
        var result = _sut.Validate(
            new CreateManuscriptCommand("", "İçerik", null, null, SubmitForReview: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateManuscriptCommand.Title));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateManuscriptCommand.ResearchAreaId));
    }
}
