using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.UnpublishManuscript;

public sealed record UnpublishManuscriptCommand(int Id) : IRequest;

public sealed class UnpublishManuscriptCommandHandler : IRequestHandler<UnpublishManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;

    public UnpublishManuscriptCommandHandler(IRepository<Manuscript> manuscripts, IUnitOfWork uow)
    {
        _manuscripts = manuscripts;
        _uow = uow;
    }

    public async Task Handle(UnpublishManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Unpublish);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
