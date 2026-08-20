using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(int Id) : IRequest;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == userId, cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException($"Bildirim bulunamadı: {request.Id}");
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
