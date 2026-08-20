using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Authorization;
using Blog.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Reviews.Commands.SubmitReview;

public sealed record SubmitReviewCommand(
    int ReviewId,
    ReviewRecommendation Recommendation,
    string Comments) : IRequest;

public sealed class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).GreaterThan(0);
        RuleFor(x => x.Recommendation).IsInEnum();
        RuleFor(x => x.Comments).NotEmpty().MaximumLength(4000);
    }
}

public sealed class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public SubmitReviewCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _db.Reviews
            .Include(r => r.Manuscript)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException($"Değerlendirme bulunamadı: {request.ReviewId}");
        }

        if (_currentUser.UserId != review.ReviewerId)
        {
            throw new ForbiddenException("Yalnızca size atanan değerlendirmeyi teslim edebilirsiniz.");
        }

        ManuscriptAccess.ApplyTransition(
            () => review.SubmitReport(request.Recommendation, request.Comments, DateTime.UtcNow));

        await _db.SaveChangesAsync(cancellationToken);

        var title = review.Manuscript?.Title ?? "Makale";
        await _notifications.NotifyUsersWithPermissionAsync(
            Permissions.Manuscripts.Decide,
            "Hakem raporu geldi",
            $"\"{title}\" için değerlendirme teslim edildi.",
            review.ManuscriptId,
            excludeUserId: review.ReviewerId,
            cancellationToken);
    }
}
