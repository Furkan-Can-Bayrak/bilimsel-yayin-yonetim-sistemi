using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.SubmitManuscript;

public sealed record SubmitManuscriptCommand(int Id) : IRequest;

public sealed class SubmitManuscriptCommandHandler : IRequestHandler<SubmitManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public SubmitManuscriptCommandHandler(
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

    public async Task Handle(SubmitManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        if (_currentUser.UserId != manuscript.AuthorId)
        {
            throw new ForbiddenException("Yalnızca kendi makalenizi gönderebilirsiniz.");
        }

        if (string.IsNullOrWhiteSpace(manuscript.Title) ||
            string.IsNullOrWhiteSpace(manuscript.Content) ||
            manuscript.ResearchAreaId is null or <= 0)
        {
            throw new ConflictException(
                "Değerlendirmeye göndermek için başlık, içerik ve araştırma alanı zorunludur.");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Submit);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersWithPermissionAsync(
            Permissions.Manuscripts.Decide,
            "Yeni makale geldi",
            $"\"{manuscript.Title}\" değerlendirmeye gönderildi.",
            manuscript.Id,
            excludeUserId: manuscript.AuthorId,
            cancellationToken);
    }
}
