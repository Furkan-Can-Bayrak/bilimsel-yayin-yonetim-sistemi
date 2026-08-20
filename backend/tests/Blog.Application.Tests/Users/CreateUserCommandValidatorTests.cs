using Blog.Application.Users.Commands.CreateUser;
using Blog.Domain.Enums;

namespace Blog.Application.Tests.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _sut.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Short_password_fails()
    {
        var result = _sut.Validate(ValidCommand() with { Password = "123" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Password));
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var result = _sut.Validate(ValidCommand() with { Email = "not-an-email" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Empty_roles_fails()
    {
        var result = _sut.Validate(ValidCommand() with { RoleIds = [] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.RoleIds));
    }

    [Fact]
    public void Invalid_orcid_fails()
    {
        var result = _sut.Validate(ValidCommand() with { Orcid = "123" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Orcid));
    }

    [Fact]
    public void Valid_orcid_passes()
    {
        var result = _sut.Validate(ValidCommand() with { Orcid = "0000-0002-1825-0097" });

        Assert.True(result.IsValid);
    }

    private static CreateUserCommand ValidCommand() => new(
        Email: "yeni@yayin.local",
        Password: "Password12",
        FirstName: "Ayşe",
        LastName: "Yılmaz",
        AcademicTitle: AcademicTitle.Dr,
        Orcid: null,
        InstitutionId: null,
        RoleIds: [1]);
}
