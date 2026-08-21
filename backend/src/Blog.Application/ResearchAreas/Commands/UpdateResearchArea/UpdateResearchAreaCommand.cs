using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Blog.Application.ResearchAreas.Commands.UpdateResearchArea;

public sealed record UpdateResearchAreaCommand(int Id, string Name) : IRequest;

public sealed class UpdateResearchAreaCommandValidator : AbstractValidator<UpdateResearchAreaCommand>
{
    public UpdateResearchAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateResearchAreaCommandHandler : IRequestHandler<UpdateResearchAreaCommand>
{
    private readonly IResearchAreaRepository _researchAreas;
    private readonly IUnitOfWork _uow;

    public UpdateResearchAreaCommandHandler(IResearchAreaRepository researchAreas, IUnitOfWork uow)
    {
        _researchAreas = researchAreas;
        _uow = uow;
    }

    public async Task Handle(UpdateResearchAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _researchAreas.GetByIdAsync(request.Id, cancellationToken);

        if (area is null)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.Id}");
        }

        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            request.Name,
            nameof(request.Name),
            s => _researchAreas.SlugExistsAsync(s, request.Id, cancellationToken),
            cancellationToken);

        area.Name = request.Name.Trim();
        area.Slug = slug;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
