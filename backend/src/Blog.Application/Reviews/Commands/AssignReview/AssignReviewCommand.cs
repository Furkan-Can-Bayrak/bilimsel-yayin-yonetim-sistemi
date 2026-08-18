using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Reviews.Commands.AssignReview;

public sealed record AssignReviewCommand(int ManuscriptId, int ReviewerId) : IRequest<int>;

public sealed class AssignReviewCommandValidator : AbstractValidator<AssignReviewCommand>
{
    public AssignReviewCommandValidator()
    {
        RuleFor(x => x.ManuscriptId).GreaterThan(0);
        RuleFor(x => x.ReviewerId).GreaterThan(0);
    }
}

public sealed class AssignReviewCommandHandler : IRequestHandler<AssignReviewCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public AssignReviewCommandHandler(IApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<int> Handle(AssignReviewCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.ManuscriptId, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.ManuscriptId}");
        }

        if (request.ReviewerId == manuscript.AuthorId)
        {
            throw new ConflictException("Makalenin yazarı hakem olarak atanamaz.");
        }

        var hasOpenReview = await _db.Reviews.AnyAsync(
            r => r.ManuscriptId == manuscript.Id && r.SubmittedAtUtc == null,
            cancellationToken);

        if (hasOpenReview)
        {
            throw new ConflictException("Bu makalede zaten açık bir hakem ataması var.");
        }

        var reviewer = await _db.Users
            .Where(u => u.Id == request.ReviewerId && u.IsActive)
            .Select(u => new { u.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (reviewer is null)
        {
            throw new NotFoundException($"Hakem bulunamadı: {request.ReviewerId}");
        }

        var canReview = await _db.Users
            .Where(u => u.Id == request.ReviewerId)
            .AnyAsync(
                u => u.UserRoles.Any(ur =>
                    ur.Role.RolePermissions.Any(rp => rp.Permission.Code == Permissions.Reviews.Submit)),
                cancellationToken);

        if (!canReview)
        {
            throw new ConflictException("Seçilen kullanıcının değerlendirme izni yok.");
        }

        ManuscriptAccess.ApplyTransition(manuscript.AssignReviewer);

        var review = new Review
        {
            ManuscriptId = manuscript.Id,
            ReviewerId = reviewer.Id,
            AssignedAtUtc = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyAsync(
            "Size makale atandı",
            $"\"{manuscript.Title}\" değerlendirmesi size atandı.",
            manuscript.Id,
            cancellationToken);

        return review.Id;
    }
}
