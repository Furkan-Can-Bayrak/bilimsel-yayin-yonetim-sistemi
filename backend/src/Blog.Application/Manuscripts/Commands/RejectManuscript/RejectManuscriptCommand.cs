using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.RejectManuscript;

public sealed record RejectManuscriptCommand(int Id) : IRequest;

public sealed class RejectManuscriptCommandHandler : IRequestHandler<RejectManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public RejectManuscriptCommandHandler(
        IRepository<Manuscript> manuscripts,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _manuscripts = manuscripts;
        _uow = uow;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(RejectManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);
        ManuscriptAccess.ApplyTransition(manuscript.Reject);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [manuscript.AuthorId],
            "Makale reddedildi",
            $"\"{manuscript.Title}\" reddedildi. Düzeltip yeniden gönderebilirsiniz.",
            manuscript.Id,
            cancellationToken);
    }
}
