using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.CreateManuscript;

public sealed record CreateManuscriptCommand(
    string Title,
    string Content,
    string? Summary,
    int? ResearchAreaId,
    bool SubmitForReview = false) : IRequest<CreateManuscriptResult>;

public sealed class CreateManuscriptCommandValidator : AbstractValidator<CreateManuscriptCommand>
{
    public CreateManuscriptCommandValidator()
    {
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

    private static bool HasAnyContent(CreateManuscriptCommand x) =>
        !string.IsNullOrWhiteSpace(x.Title) ||
        !string.IsNullOrWhiteSpace(x.Content) ||
        !string.IsNullOrWhiteSpace(x.Summary) ||
        x.ResearchAreaId is > 0;
}

public sealed class CreateManuscriptCommandHandler
    : IRequestHandler<CreateManuscriptCommand, CreateManuscriptResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public CreateManuscriptCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<CreateManuscriptResult> Handle(
        CreateManuscriptCommand request,
        CancellationToken cancellationToken)
    {
        var authorId = _currentUser.RequireUserId();
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
            s => _db.Manuscripts.AnyAsync(m => m.Slug == s, cancellationToken),
            cancellationToken);

        var manuscript = new Manuscript
        {
            Title = title,
            Content = request.Content ?? string.Empty,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            ResearchAreaId = researchAreaId,
            AuthorId = authorId,
            Slug = slug
        };

        if (request.SubmitForReview)
        {
            if (!_currentUser.HasPermission(Permissions.Manuscripts.Submit))
            {
                throw new ForbiddenException("Makaleyi değerlendirmeye gönderme izniniz yok.");
            }

            ManuscriptAccess.ApplyTransition(manuscript.Submit);
        }

        _db.Manuscripts.Add(manuscript);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.SubmitForReview)
        {
            await _notifications.NotifyUsersWithPermissionAsync(
                Permissions.Manuscripts.Decide,
                "Yeni makale geldi",
                $"\"{manuscript.Title}\" değerlendirmeye gönderildi.",
                manuscript.Id,
                excludeUserId: authorId,
                cancellationToken);
        }

        return new CreateManuscriptResult(manuscript.Id, manuscript.Slug);
    }
}
