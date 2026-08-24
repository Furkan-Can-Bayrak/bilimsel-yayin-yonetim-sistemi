using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;

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
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public UpdateUserRolesCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUnitOfWork uow)
    {
        _users = users;
        _roles = roles;
        _uow = uow;
    }

    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var existingRoleCount = await _roles.CountByIdsAsync(roleIds, cancellationToken);

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
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
