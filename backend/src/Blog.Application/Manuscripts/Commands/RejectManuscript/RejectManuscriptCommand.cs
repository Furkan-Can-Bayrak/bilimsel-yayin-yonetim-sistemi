using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.RejectManuscript;

public sealed record RejectManuscriptCommand(int Id, string Reason) : IRequest;

public sealed class RejectManuscriptCommandValidator : AbstractValidator<RejectManuscriptCommand>
{
    public RejectManuscriptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RejectManuscriptCommandHandler : IRequestHandler<RejectManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IReviewRepository _reviews;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public RejectManuscriptCommandHandler(
        IRepository<Manuscript> manuscripts,
        IReviewRepository reviews,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        INotificationService notifications)
    {
        _manuscripts = manuscripts;
        _reviews = reviews;
        _uow = uow;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(RejectManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);
        var hasOpenReview = await _reviews.HasOpenForManuscriptAsync(manuscript.Id, cancellationToken);
        ManuscriptAccess.EnsureNoOpenReview(hasOpenReview);
        ManuscriptAccess.ApplyTransition(() => manuscript.Reject(request.Reason));
        await _uow.SaveChangesAsync(cancellationToken);

        await _notifications.NotifyUsersAsync(
            [manuscript.AuthorId],
            "Makale reddedildi",
            $"\"{manuscript.Title}\" reddedildi. Gerekçe: {manuscript.RejectionReason}",
            manuscript.Id,
            cancellationToken);
    }
}
