using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using Blog.Application.Manuscripts;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Blog.Application.Manuscripts.Commands.PublishManuscript;

public sealed record PublishManuscriptCommand(int Id) : IRequest;

public sealed class PublishManuscriptCommandHandler : IRequestHandler<PublishManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _email;
    private readonly INotificationService _notifications;
    private readonly EmailOptions _emailOptions;

    public PublishManuscriptCommandHandler(
        IRepository<Manuscript> manuscripts,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IEmailService email,
        INotificationService notifications,
        IOptions<EmailOptions> emailOptions)
    {
        _manuscripts = manuscripts;
        _uow = uow;
        _currentUser = currentUser;
        _email = email;
        _notifications = notifications;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(PublishManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);
        ManuscriptAccess.ApplyTransition(() => manuscript.Publish(DateTime.UtcNow));
        await _uow.SaveChangesAsync(cancellationToken);

        await ManuscriptPublication.NotifyPublishedAsync(
            _notifications,
            _email,
            _emailOptions,
            manuscript,
            cancellationToken);
    }
}
