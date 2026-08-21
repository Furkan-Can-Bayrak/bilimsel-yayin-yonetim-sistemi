using Blog.Application.Common;
using Blog.Application.Common.Interfaces;
using Blog.Application.ResearchAreas.Dtos;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;

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
    private readonly IResearchAreaRepository _researchAreas;
    private readonly IUnitOfWork _uow;

    public CreateResearchAreaCommandHandler(IResearchAreaRepository researchAreas, IUnitOfWork uow)
    {
        _researchAreas = researchAreas;
        _uow = uow;
    }

    public async Task<CreateResearchAreaResult> Handle(
        CreateResearchAreaCommand request,
        CancellationToken cancellationToken)
    {
        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            request.Name,
            nameof(request.Name),
            s => _researchAreas.SlugExistsAsync(s, cancellationToken: cancellationToken),
            cancellationToken);

        var area = new ResearchArea
        {
            Name = request.Name.Trim(),
            Slug = slug
        };

        await _researchAreas.AddAsync(area, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateResearchAreaResult(area.Id, area.Slug);
    }
}
