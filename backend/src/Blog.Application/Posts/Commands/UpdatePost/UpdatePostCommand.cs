using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blog.Application.Posts.Commands.UpdatePost;

public sealed record UpdatePostCommand(
    int Id,
    string Title,
    string Content,
    string? Summary,
    int CategoryId,
    bool IsPublished,
    string? Slug) : IRequest;

public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Slug).MaximumLength(220).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly INotificationService _notifications;
    private readonly EmailOptions _emailOptions;

    public UpdatePostCommandHandler(
        IApplicationDbContext db,
        IEmailService email,
        INotificationService notifications,
        IOptions<EmailOptions> emailOptions)
    {
        _db = db;
        _email = email;
        _notifications = notifications;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Yazı bulunamadı: {request.Id}");
        }

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException($"Kategori bulunamadı: {request.CategoryId}");
        }

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateFromTitle(request.Title)
            : SlugHelper.GenerateFromTitle(request.Slug);

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.Posts.AnyAsync(p => p.Slug == s && p.Id != request.Id, cancellationToken),
            baseSlug,
            cancellationToken);

        var wasPublished = post.IsPublished;

        post.Title = request.Title.Trim();
        post.Content = request.Content;
        post.Summary = request.Summary;
        post.CategoryId = request.CategoryId;
        post.Slug = slug;
        post.IsPublished = request.IsPublished;

        if (request.IsPublished && !wasPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }
        else if (!request.IsPublished)
        {
            post.PublishedAt = null;
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Taslaktan yayına alındığında bildirim + e-posta
        if (request.IsPublished && !wasPublished)
        {
            await _notifications.NotifyAsync(
                "Yazı yayınlandı",
                $"\"{post.Title}\" yayınlandı.",
                post.Id,
                cancellationToken);

            await _email.SendAsync(
                _emailOptions.NotifyTo,
                $"Yeni yazı: {post.Title}",
                $"Yazı yayınlandı.\nBaşlık: {post.Title}\nSlug: {post.Slug}",
                cancellationToken);
        }
    }
}
