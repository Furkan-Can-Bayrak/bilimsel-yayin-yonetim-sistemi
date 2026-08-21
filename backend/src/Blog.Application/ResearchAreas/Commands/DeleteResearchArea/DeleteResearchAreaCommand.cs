using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Blog.Application.ResearchAreas.Commands.DeleteResearchArea;

public sealed record DeleteResearchAreaCommand(int Id) : IRequest;

public sealed class DeleteResearchAreaCommandValidator : AbstractValidator<DeleteResearchAreaCommand>
{
    public DeleteResearchAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public sealed class DeleteResearchAreaCommandHandler : IRequestHandler<DeleteResearchAreaCommand>
{
    private readonly IResearchAreaRepository _researchAreas;
    private readonly IManuscriptRepository _manuscripts;
    private readonly IUnitOfWork _uow;

    public DeleteResearchAreaCommandHandler(
        IResearchAreaRepository researchAreas,
        IManuscriptRepository manuscripts,
        IUnitOfWork uow)
    {
        _researchAreas = researchAreas;
        _manuscripts = manuscripts;
        _uow = uow;
    }

    public async Task Handle(DeleteResearchAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _researchAreas.GetByIdAsync(request.Id, cancellationToken);

        if (area is null)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.Id}");
        }

        var hasManuscripts = await _manuscripts.AnyInResearchAreaAsync(request.Id, cancellationToken);

        if (hasManuscripts)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Id"] = ["Bu alana bağlı makaleler var; önce makaleleri silin veya taşıyın."]
            });
        }

        _researchAreas.Remove(area);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
