using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using FluentValidation;
using MediatR;

namespace Blog.Application.Reviews.Commands.WithdrawReview;

public sealed record WithdrawReviewCommand(int ReviewId) : IRequest;

public sealed class WithdrawReviewCommandValidator : AbstractValidator<WithdrawReviewCommand>
{
    public WithdrawReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).GreaterThan(0);
    }
}

public sealed class WithdrawReviewCommandHandler : IRequestHandler<WithdrawReviewCommand>
{
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public WithdrawReviewCommandHandler(
        IReviewRepository reviews,
        ICurrentUser currentUser,
        IUnitOfWork uow,
        INotificationService notifications)
    {
        _reviews = reviews;
        _currentUser = currentUser;
        _uow = uow;
        _notifications = notifications;
    }

    public async Task Handle(WithdrawReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviews.GetByIdWithManuscriptAsync(request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException($"Değerlendirme bulunamadı: {request.ReviewId}");
        }

        var manuscript = review.Manuscript
            ?? throw new NotFoundException($"Makale bulunamadı: {review.ManuscriptId}");

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);

        if (review.IsSubmitted)
        {
            throw new ConflictException("Teslim edilmiş değerlendirme geri alınamaz.");
        }

        var reviewerId = review.ReviewerId;
        var title = manuscript.Title;
        var hasSubmitted = await _reviews.HasSubmittedForManuscriptAsync(
            manuscript.Id,
            cancellationToken);

        _reviews.Remove(review);

        if (!hasSubmitted)
        {
            ManuscriptAccess.ApplyTransition(manuscript.ReturnToSubmitted);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [reviewerId],
            "Hakem ataması geri alındı",
            $"\"{title}\" değerlendirmesi geri alındı.",
            manuscript.Id,
            cancellationToken);
    }
}
