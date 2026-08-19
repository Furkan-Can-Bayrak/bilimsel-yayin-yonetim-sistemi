using Blog.Application.Common;
using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.ResearchAreas.Commands.CreateResearchArea;

public sealed record CreateResearchAreaCommand(string Name, string? Slug)
    : IRequest<CreateResearchAreaResult>;

public sealed class CreateResearchAreaCommandValidator : AbstractValidator<CreateResearchAreaCommand>
{
    public CreateResearchAreaCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class CreateResearchAreaCommandHandler
    : IRequestHandler<CreateResearchAreaCommand, CreateResearchAreaResult>
{
    private readonly IApplicationDbContext _db;

    public CreateResearchAreaCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CreateResearchAreaResult> Handle(
        CreateResearchAreaCommand request,
        CancellationToken cancellationToken)
    {
        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name, nameof(request.Name))
            : SlugHelper.GenerateSlug(request.Slug, nameof(request.Slug));

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.ResearchAreas.AnyAsync(a => a.Slug == s, cancellationToken),
            baseSlug,
            cancellationToken);

        var area = new ResearchArea
        {
            Name = request.Name.Trim(),
            Slug = slug
        };

        _db.ResearchAreas.Add(area);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateResearchAreaResult(area.Id, area.Slug);
    }
}
