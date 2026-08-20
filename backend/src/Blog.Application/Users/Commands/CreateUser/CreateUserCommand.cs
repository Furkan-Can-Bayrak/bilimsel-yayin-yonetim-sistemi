using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Users.Dtos;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    AcademicTitle AcademicTitle,
    string? Orcid,
    int InstitutionId,
    IReadOnlyList<int> RoleIds) : IRequest<CreateUserResult>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly HashSet<AcademicTitle> AllowedTitles =
    [
        AcademicTitle.ProfDr,
        AcademicTitle.DocDr,
        AcademicTitle.DrOgrUyesi,
        AcademicTitle.OgrGor,
        AcademicTitle.ArsGor,
        AcademicTitle.Dr
    ];

    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.AcademicTitle)
            .Must(AllowedTitles.Contains)
            .WithMessage("Geçerli bir akademik unvan seçin.");
        RuleFor(x => x.InstitutionId)
            .GreaterThan(0)
            .WithMessage("Kurum zorunludur.");
        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("En az bir rol seçilmelidir.");
        RuleFor(x => x.Orcid)
            .Length(19)
            .Matches(@"^\d{4}-\d{4}-\d{4}-\d{3}[\dX]$")
            .When(x => !string.IsNullOrWhiteSpace(x.Orcid));
    }
}

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _email;

    public CreateUserCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher<User> passwordHasher,
        IEmailService email)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _email = email;
    }

    public async Task<CreateUserResult> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions
            .FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken);

        if (institution is null)
        {
            throw new NotFoundException($"Kurum bulunamadı: {request.InstitutionId}");
        }

        if (string.IsNullOrWhiteSpace(institution.EmailDomain))
        {
            throw new ConflictException("Seçilen kurumun e-posta alanı tanımlı değil.");
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var existingRoleCount = await _db.Roles
            .CountAsync(r => roleIds.Contains(r.Id), cancellationToken);

        if (existingRoleCount != roleIds.Length)
        {
            throw new ConflictException("Seçilen rollerden biri bulunamadı.");
        }

        var orcid = string.IsNullOrWhiteSpace(request.Orcid)
            ? null
            : request.Orcid.Trim();

        if (orcid is not null)
        {
            var orcidTaken = await _db.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Orcid == orcid, cancellationToken);

            if (orcidTaken)
            {
                throw new ConflictException("Bu ORCID zaten kayıtlı.");
            }
        }

        string email;
        try
        {
            email = await UserEmailHelper.BuildUniqueEmailAsync(
                request.FirstName,
                request.LastName,
                institution.EmailDomain,
                candidate => _db.Users
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Email == candidate, cancellationToken),
                cancellationToken);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(ex.Message);
        }

        var password = UserEmailHelper.GeneratePassword();

        var user = new User
        {
            Email = email,
            AcademicTitle = request.AcademicTitle,
            InstitutionId = institution.Id,
            Orcid = orcid,
            IsActive = true,
            SecurityVersion = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.SetName(request.FirstName, request.LastName);
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(new UserRole { RoleId = roleId });
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendAsync(
            email,
            "BYYS hesap bilgileriniz",
            $"""
            Merhaba {user.DisplayName},

            Bilimsel Yayın Yönetim Sistemi hesabınız oluşturuldu.

            E-posta: {email}
            Geçici şifre: {password}

            Giriş yaptıktan sonra şifrenizi değiştirmeniz önerilir.
            """,
            cancellationToken);

        return new CreateUserResult(user.Id, user.Email);
    }
}
