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
    int ResearchAreaId) : IRequest<CreateManuscriptResult>;

public sealed class CreateManuscriptCommandValidator : AbstractValidator<CreateManuscriptCommand>
{
    public CreateManuscriptCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);
        RuleFor(x => x.ResearchAreaId).GreaterThan(0);
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

        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            request.Title,
            nameof(request.Title),
            s => _db.Manuscripts.AnyAsync(m => m.Slug == s, cancellationToken),
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
