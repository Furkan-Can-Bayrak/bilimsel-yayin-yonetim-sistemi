using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.RejectManuscript;

public sealed record RejectManuscriptCommand(int Id) : IRequest;

public sealed class RejectManuscriptCommandHandler : IRequestHandler<RejectManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public RejectManuscriptCommandHandler(IApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task Handle(RejectManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Reject);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyAsync(
            "Makale reddedildi",
            $"\"{manuscript.Title}\" reddedildi. Yazar düzeltip yeniden gönderebilir.",
            manuscript.Id,
            cancellationToken);
    }
}
