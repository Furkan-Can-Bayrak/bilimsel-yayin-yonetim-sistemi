using Blog.Application.Posts.Commands.CreatePost;

namespace Blog.Application.Tests.Posts;

public class CreatePostCommandValidatorTests
{
    private readonly CreatePostCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _sut.Validate(
            new CreatePostCommand("İlk yazı", "İçerik buraya", "Özet", 1, true, null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_title_fails()
    {
        var result = _sut.Validate(
            new CreatePostCommand("", "İçerik", null, 1, false, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePostCommand.Title));
    }

    [Fact]
    public void CategoryId_must_be_greater_than_zero()
    {
        var result = _sut.Validate(
            new CreatePostCommand("Başlık", "İçerik", null, 0, true, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePostCommand.CategoryId));
    }
}
