using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
