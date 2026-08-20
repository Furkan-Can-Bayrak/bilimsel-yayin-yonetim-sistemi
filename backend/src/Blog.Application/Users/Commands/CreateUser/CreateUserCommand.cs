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
    string Email,
    string Password,
    string FirstName,
    string LastName,
    AcademicTitle AcademicTitle,
    string? Orcid,
    int? InstitutionId,
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
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.AcademicTitle)
            .Must(AllowedTitles.Contains)
            .WithMessage("Geçerli bir akademik unvan seçin.");
        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("En az bir rol seçilmelidir.");
        RuleFor(x => x.Orcid)
            .Length(19)
            .Matches(@"^\d{4}-\d{4}-\d{4}-\d{3}[\dX]$")
            .When(x => !string.IsNullOrWhiteSpace(x.Orcid));
        RuleFor(x => x.InstitutionId)
            .GreaterThan(0)
            .When(x => x.InstitutionId is not null);
    }
}

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public CreateUserCommandHandler(IApplicationDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResult> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException("Bu e-posta adresi zaten kayıtlı.");
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var existingRoleCount = await _db.Roles
            .CountAsync(r => roleIds.Contains(r.Id), cancellationToken);

        if (existingRoleCount != roleIds.Length)
        {
            throw new ConflictException("Seçilen rollerden biri bulunamadı.");
        }

        if (request.InstitutionId is int institutionId)
        {
            var institutionExists = await _db.Institutions
                .AnyAsync(i => i.Id == institutionId, cancellationToken);

            if (!institutionExists)
            {
                throw new NotFoundException($"Kurum bulunamadı: {institutionId}");
            }
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

        var user = new User
        {
            Email = email,
            AcademicTitle = request.AcademicTitle,
            InstitutionId = request.InstitutionId,
            Orcid = orcid,
            IsActive = true,
            SecurityVersion = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.SetName(request.FirstName, request.LastName);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(new UserRole { RoleId = roleId });
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(user.Id, user.Email);
    }
}
