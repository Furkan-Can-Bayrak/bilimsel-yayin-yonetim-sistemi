using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.AcceptManuscript;

public sealed record AcceptManuscriptCommand(int Id) : IRequest;

public sealed class AcceptManuscriptCommandHandler : IRequestHandler<AcceptManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public AcceptManuscriptCommandHandler(
        IRepository<Manuscript> manuscripts,
        IUnitOfWork uow,
        INotificationService notifications)
    {
        _manuscripts = manuscripts;
        _uow = uow;
        _notifications = notifications;
    }

    public async Task Handle(AcceptManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Accept);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [manuscript.AuthorId],
            "Makale kabul edildi",
            $"\"{manuscript.Title}\" yayına hazır olarak kabul edildi.",
            manuscript.Id,
            cancellationToken);
    }
}
