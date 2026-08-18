using Blog.Domain.Entities;

namespace Blog.Application.Tests.Auth;

public class UserNameTests
{
    [Fact]
    public void SetName_stores_trimmed_parts()
    {
        var user = new User();

        user.SetName("  Elif ", " Demir ");

        Assert.Equal("Elif", user.FirstName);
        Assert.Equal("Demir", user.LastName);
        Assert.Equal("Elif Demir", user.DisplayName);
    }

    [Fact]
    public void SetName_requires_first_and_last()
    {
        var user = new User();

        Assert.Throws<ArgumentException>(() => user.SetName(" ", "Demir"));
        Assert.Throws<ArgumentException>(() => user.SetName("Elif", "\t"));
    }
}
