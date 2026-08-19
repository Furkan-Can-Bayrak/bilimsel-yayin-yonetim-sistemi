using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.ResearchAreas.Commands.UpdateResearchArea;

public sealed record UpdateResearchAreaCommand(int Id, string Name, string? Slug) : IRequest;

public sealed class UpdateResearchAreaCommandValidator : AbstractValidator<UpdateResearchAreaCommand>
{
    public UpdateResearchAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class UpdateResearchAreaCommandHandler : IRequestHandler<UpdateResearchAreaCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateResearchAreaCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateResearchAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _db.ResearchAreas
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (area is null)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.Id}");
        }

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name, nameof(request.Name))
            : SlugHelper.GenerateSlug(request.Slug, nameof(request.Slug));

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.ResearchAreas.AnyAsync(a => a.Slug == s && a.Id != request.Id, cancellationToken),
            baseSlug,
            cancellationToken);

        area.Name = request.Name.Trim();
        area.Slug = slug;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
