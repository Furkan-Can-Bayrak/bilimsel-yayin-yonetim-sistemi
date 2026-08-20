using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.AcceptManuscript;

public sealed record AcceptManuscriptCommand(int Id) : IRequest;

public sealed class AcceptManuscriptCommandHandler : IRequestHandler<AcceptManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public AcceptManuscriptCommandHandler(IApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task Handle(AcceptManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Accept);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [manuscript.AuthorId],
            "Makale kabul edildi",
            $"\"{manuscript.Title}\" yayına hazır olarak kabul edildi.",
            manuscript.Id,
            cancellationToken);
    }
}
