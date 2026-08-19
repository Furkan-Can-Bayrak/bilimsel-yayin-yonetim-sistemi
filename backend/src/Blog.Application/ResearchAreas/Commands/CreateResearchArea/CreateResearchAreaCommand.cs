using Blog.Application.Common;
using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.ResearchAreas.Commands.CreateResearchArea;

public sealed record CreateResearchAreaCommand(string Name)
    : IRequest<CreateResearchAreaResult>;

public sealed class CreateResearchAreaCommandValidator : AbstractValidator<CreateResearchAreaCommand>
{
    public CreateResearchAreaCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
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
        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            request.Name,
            nameof(request.Name),
            s => _db.ResearchAreas.AnyAsync(a => a.Slug == s, cancellationToken),
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
