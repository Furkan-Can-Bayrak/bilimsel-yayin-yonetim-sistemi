using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blog.Application.Manuscripts.Commands.PublishManuscript;

public sealed record PublishManuscriptCommand(int Id) : IRequest;

public sealed class PublishManuscriptCommandHandler : IRequestHandler<PublishManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly INotificationService _notifications;
    private readonly EmailOptions _emailOptions;

    public PublishManuscriptCommandHandler(
        IApplicationDbContext db,
        IEmailService email,
        INotificationService notifications,
        IOptions<EmailOptions> emailOptions)
    {
        _db = db;
        _email = email;
        _notifications = notifications;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(PublishManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        if (!manuscript.Publish(DateTime.UtcNow))
        {
            return;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await ManuscriptPublication.NotifyPublishedAsync(
            _notifications,
            _email,
            _emailOptions,
            manuscript,
            cancellationToken);
    }
}
