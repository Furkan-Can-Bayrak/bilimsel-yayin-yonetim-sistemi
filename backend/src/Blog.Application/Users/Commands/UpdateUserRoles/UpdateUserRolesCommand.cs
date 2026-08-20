using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Commands.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(int UserId, int RoleId) : IRequest;

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoleId).GreaterThan(0);
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

        var roleExists = await _db.Roles
            .AnyAsync(r => r.Id == request.RoleId, cancellationToken);

        if (!roleExists)
        {
            throw new ConflictException("Seçilen rol bulunamadı.");
        }

        if (user.UserRoles.Count == 1 && user.UserRoles.First().RoleId == request.RoleId)
        {
            return;
        }

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = request.RoleId
        });
        user.SecurityVersion += 1;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
