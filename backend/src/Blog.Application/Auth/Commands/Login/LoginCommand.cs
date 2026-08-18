using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private const string InvalidCredentials = "E-posta veya şifre hatalı.";

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
        var email = request.Email.Trim().ToLowerInvariant();

        // Silinmiş kullanıcılar query filter sayesinde sorguya hiç girmez.
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException(InvalidCredentials);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(InvalidCredentials);
        }

        // Şifre doğrulandıktan sonra bakıyoruz: aksi halde bu mesaj, hesabın
        // var olup olmadığını dışarıya sızdırırdı.
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış. Lütfen editör ile iletişime geçin.");
        }

        // Roles üzerinden başlıyoruz: silinmiş roller query filter ile kendiliğinden düşer,
        // böylece soft delete kuralını burada tekrar yazmak gerekmiyor.
        var roles = await _db.Roles
            .Where(r => r.UserRoles.Any(ur => ur.UserId == user.Id))
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();

        var permissions = await _db.Permissions
            .Where(p => p.RolePermissions.Any(rp => roleIds.Contains(rp.RoleId)))
            .OrderBy(p => p.Code)
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var roleNames = roles.Select(r => r.Name).ToArray();

        var token = _jwt.CreateToken(user, roleNames, permissions);

        return new LoginResponse(
            token.Value,
            token.ExpiresAtUtc,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roleNames,
            permissions);
    }
}
