using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.SubmitManuscript;

public sealed record SubmitManuscriptCommand(int Id) : IRequest;

public sealed class SubmitManuscriptCommandHandler : IRequestHandler<SubmitManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public SubmitManuscriptCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(SubmitManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

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
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersWithPermissionAsync(
            Permissions.Manuscripts.Decide,
            "Yeni makale geldi",
            $"\"{manuscript.Title}\" değerlendirmeye gönderildi.",
            manuscript.Id,
            excludeUserId: manuscript.AuthorId,
            cancellationToken);
    }
}
