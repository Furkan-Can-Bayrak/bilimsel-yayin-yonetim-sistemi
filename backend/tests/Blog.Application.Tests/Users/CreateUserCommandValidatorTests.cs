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
    public void Missing_institution_fails()
    {
        var result = _sut.Validate(ValidCommand() with { InstitutionId = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.InstitutionId));
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
        FirstName: "Ayşe",
        LastName: "Yılmaz",
        AcademicTitle: AcademicTitle.Dr,
        Orcid: null,
        InstitutionId: 1,
        RoleIds: [1]);
}
