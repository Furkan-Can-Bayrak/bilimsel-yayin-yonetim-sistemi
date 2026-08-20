using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Users.Commands.UpdateUserActiveStatus;

public sealed record UpdateUserActiveStatusCommand(int UserId, bool IsActive) : IRequest;

public sealed class UpdateUserActiveStatusCommandValidator
    : AbstractValidator<UpdateUserActiveStatusCommand>
{
    public UpdateUserActiveStatusCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public sealed class UpdateUserActiveStatusCommandHandler
    : IRequestHandler<UpdateUserActiveStatusCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateUserActiveStatusCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateUserActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }

        if (!request.IsActive && _currentUser.UserId == user.Id)
        {
            throw new ConflictException("Kendi hesabınızı pasifleştiremezsiniz.");
        }

        if (user.IsActive == request.IsActive)
        {
            return;
        }

        user.IsActive = request.IsActive;
        user.SecurityVersion += 1;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
