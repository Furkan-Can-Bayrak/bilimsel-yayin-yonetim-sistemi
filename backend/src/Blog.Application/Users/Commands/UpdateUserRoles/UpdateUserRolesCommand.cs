using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Commands.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(int UserId, IReadOnlyList<int> RoleIds) : IRequest;

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("En az bir rol seçilmelidir.");
    }
}

public sealed class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateUserRolesCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var existingRoleCount = await _db.Roles
            .CountAsync(r => roleIds.Contains(r.Id), cancellationToken);

        if (existingRoleCount != roleIds.Length)
        {
            throw new ConflictException("Seçilen rollerden biri bulunamadı.");
        }

        var current = user.UserRoles.Select(ur => ur.RoleId).OrderBy(id => id).ToArray();
        var next = roleIds.OrderBy(id => id).ToArray();
        if (current.SequenceEqual(next))
        {
            return;
        }

        user.UserRoles.Clear();
        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        user.SecurityVersion += 1;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
