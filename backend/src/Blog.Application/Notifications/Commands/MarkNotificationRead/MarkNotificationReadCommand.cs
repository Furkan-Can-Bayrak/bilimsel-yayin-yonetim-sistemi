using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using MediatR;

namespace Blog.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(int Id) : IRequest;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notifications,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _notifications = notifications;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var notification = await _notifications.GetByIdForUserAsync(
            request.Id,
            userId,
            cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException($"Bildirim bulunamadı: {request.Id}");
        }

        notification.IsRead = true;
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
