using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;

public sealed record LoginResponse(string AccessToken, string Username, string Role, DateTime ExpiresAtUtc);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IJwtTokenService jwt,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Kullanıcı adı veya şifre hatalı.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Kullanıcı adı veya şifre hatalı.");
        }

        var token = _jwt.CreateToken(user);
        var expires = DateTime.UtcNow.AddHours(8);

        return new LoginResponse(token, user.Username, user.Role, expires);
    }
}
