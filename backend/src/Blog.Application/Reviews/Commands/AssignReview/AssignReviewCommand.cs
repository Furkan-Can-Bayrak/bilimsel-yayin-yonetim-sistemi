using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;

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
    private readonly IManuscriptRepository _manuscripts;
    private readonly IReviewRepository _reviews;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public AssignReviewCommandHandler(
        IManuscriptRepository manuscripts,
        IReviewRepository reviews,
        IUserRepository users,
        ICurrentUser currentUser,
        IUnitOfWork uow,
        INotificationService notifications)
    {
        _manuscripts = manuscripts;
        _reviews = reviews;
        _users = users;
        _currentUser = currentUser;
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<int> Handle(AssignReviewCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.ManuscriptId, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.ManuscriptId}");
        }

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);

        if (request.ReviewerId == manuscript.AuthorId)
        {
            throw new ConflictException("Makalenin yazarı hakem olarak atanamaz.");
        }

        var hasOpenReview = await _reviews.HasOpenForManuscriptAsync(
            manuscript.Id,
            cancellationToken);

        if (hasOpenReview)
        {
            throw new ConflictException("Bu makalede zaten açık bir hakem ataması var.");
        }

        var reviewer = await _users.GetByIdAsync(request.ReviewerId, cancellationToken);

        if (reviewer is null || !reviewer.IsActive)
        {
            throw new NotFoundException($"Hakem bulunamadı: {request.ReviewerId}");
        }

        var canReview = await _users.HasPermissionAsync(
            reviewer.Id,
            Permissions.Reviews.Submit,
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

        await _reviews.AddAsync(review, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [reviewer.Id],
            "Size makale atandı",
            $"\"{manuscript.Title}\" değerlendirmesi size atandı.",
            manuscript.Id,
            cancellationToken);

        return review.Id;
    }
}
