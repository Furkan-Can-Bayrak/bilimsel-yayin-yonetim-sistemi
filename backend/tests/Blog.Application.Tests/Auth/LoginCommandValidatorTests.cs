using Blog.Application.Auth.Commands.Login;

namespace Blog.Application.Tests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public void Valid_login_passes()
    {
        var result = _sut.Validate(new LoginCommand("admin@yayin.local", "Password12"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Short_password_fails()
    {
        var result = _sut.Validate(new LoginCommand("admin@yayin.local", "123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
