using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using Blog.Application.Posts.Dtos;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blog.Application.Posts.Commands.CreatePost;

public sealed record CreatePostCommand(
    string Title,
    string Content,
    string? Summary,
    int CategoryId,
    bool IsPublished,
    string? Slug) : IRequest<CreatePostResult>;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Slug).MaximumLength(220).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly INotificationService _notifications;
    private readonly EmailOptions _emailOptions;

    public CreatePostCommandHandler(
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

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
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
            s => _db.Posts.AnyAsync(p => p.Slug == s, cancellationToken),
            baseSlug,
            cancellationToken);

        var post = new Post
        {
            Title = request.Title.Trim(),
            Content = request.Content,
            Summary = request.Summary,
            CategoryId = request.CategoryId,
            IsPublished = request.IsPublished,
            Slug = slug,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync(cancellationToken);

        if (post.IsPublished)
        {
            await NotifyPublishedAsync(post, cancellationToken);
        }

        return new CreatePostResult(post.Id, post.Slug);
    }

    private async Task NotifyPublishedAsync(Post post, CancellationToken cancellationToken)
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
