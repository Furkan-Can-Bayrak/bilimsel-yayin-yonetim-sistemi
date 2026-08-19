using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.CreateManuscript;

public sealed record CreateManuscriptCommand(
    string Title,
    string Content,
    string? Summary,
    int ResearchAreaId,
    string? Slug) : IRequest<CreateManuscriptResult>;

public sealed class CreateManuscriptCommandValidator : AbstractValidator<CreateManuscriptCommand>
{
    public CreateManuscriptCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);
        RuleFor(x => x.ResearchAreaId).GreaterThan(0);
        RuleFor(x => x.Slug).MaximumLength(220).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class CreateManuscriptCommandHandler
    : IRequestHandler<CreateManuscriptCommand, CreateManuscriptResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateManuscriptCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateManuscriptResult> Handle(
        CreateManuscriptCommand request,
        CancellationToken cancellationToken)
    {
        var authorId = _currentUser.RequireUserId();

        var areaExists = await _db.ResearchAreas
            .AnyAsync(a => a.Id == request.ResearchAreaId, cancellationToken);

        if (!areaExists)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.ResearchAreaId}");
        }

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Title, nameof(request.Title))
            : SlugHelper.GenerateSlug(request.Slug, nameof(request.Slug));

        // {slug} rotası ile GET .../admin çakışmasın.
        if (baseSlug == "admin")
        {
            baseSlug = "makale";
        }

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.Manuscripts.AnyAsync(m => m.Slug == s, cancellationToken),
            baseSlug,
            cancellationToken);

        var manuscript = new Manuscript
        {
            Title = request.Title.Trim(),
            Content = request.Content,
            Summary = request.Summary,
            ResearchAreaId = request.ResearchAreaId,
            AuthorId = authorId,
            Slug = slug
        };

        _db.Manuscripts.Add(manuscript);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateManuscriptResult(manuscript.Id, manuscript.Slug);
    }
}
