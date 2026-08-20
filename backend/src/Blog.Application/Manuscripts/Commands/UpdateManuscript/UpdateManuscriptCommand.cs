using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.UpdateManuscript;

public sealed record UpdateManuscriptCommand(
    int Id,
    string Title,
    string Content,
    string? Summary,
    int? ResearchAreaId,
    bool SubmitForReview = false) : IRequest;

public sealed class UpdateManuscriptCommandValidator : AbstractValidator<UpdateManuscriptCommand>
{
    public UpdateManuscriptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);

        RuleFor(x => x)
            .Must(HasAnyContent)
            .WithMessage("Taslak için en az bir alan doldurulmalıdır.")
            .WithName("Manuscript");

        When(x => x.SubmitForReview, () =>
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık zorunludur.");
            RuleFor(x => x.Content).NotEmpty().WithMessage("İçerik zorunludur.");
            RuleFor(x => x.ResearchAreaId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Araştırma alanı zorunludur.");
        });

        When(x => x.ResearchAreaId is not null, () =>
        {
            RuleFor(x => x.ResearchAreaId!.Value).GreaterThan(0);
        });
    }

    private static bool HasAnyContent(UpdateManuscriptCommand x) =>
        !string.IsNullOrWhiteSpace(x.Title) ||
        !string.IsNullOrWhiteSpace(x.Content) ||
        !string.IsNullOrWhiteSpace(x.Summary) ||
        x.ResearchAreaId is > 0;
}

public sealed class UpdateManuscriptCommandHandler : IRequestHandler<UpdateManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public UpdateManuscriptCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(UpdateManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        if (!ManuscriptAccess.CanUpdate(manuscript.AuthorId, _currentUser))
        {
            throw new ForbiddenException("Yalnızca kendi makalenizi düzenleyebilirsiniz.");
        }

        if (!ManuscriptAccess.CanEditContent(manuscript, _currentUser))
        {
            throw new ConflictException("Bu durumdayken makale düzenlenemez.");
        }

        var researchAreaId = request.ResearchAreaId is > 0 ? request.ResearchAreaId : null;

        if (researchAreaId is int areaId)
        {
            var areaExists = await _db.ResearchAreas
                .AnyAsync(a => a.Id == areaId, cancellationToken);

            if (!areaExists)
            {
                throw new NotFoundException($"Araştırma alanı bulunamadı: {areaId}");
            }
        }

        var title = (request.Title ?? string.Empty).Trim();
        var slugSource = string.IsNullOrWhiteSpace(title) ? "taslak" : title;

        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            slugSource,
            nameof(request.Title),
            s => _db.Manuscripts.AnyAsync(m => m.Slug == s && m.Id != request.Id, cancellationToken),
            cancellationToken);

        manuscript.Title = title;
        manuscript.Content = request.Content ?? string.Empty;
        manuscript.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        manuscript.ResearchAreaId = researchAreaId;
        manuscript.Slug = slug;

        if (request.SubmitForReview)
        {
            if (!_currentUser.HasPermission(Permissions.Manuscripts.Submit) ||
                _currentUser.UserId != manuscript.AuthorId)
            {
                throw new ForbiddenException("Yalnızca kendi makalenizi gönderebilirsiniz.");
            }

            ManuscriptAccess.ApplyTransition(manuscript.Submit);
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (request.SubmitForReview)
        {
            await _notifications.NotifyUsersWithPermissionAsync(
                Permissions.Manuscripts.Decide,
                "Yeni makale geldi",
                $"\"{manuscript.Title}\" değerlendirmeye gönderildi.",
                manuscript.Id,
                excludeUserId: manuscript.AuthorId,
                cancellationToken);
        }
    }
}
