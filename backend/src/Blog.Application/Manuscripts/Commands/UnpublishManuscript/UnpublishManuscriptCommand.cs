using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Manuscripts.Commands.UnpublishManuscript;

public sealed record UnpublishManuscriptCommand(int Id) : IRequest;

public sealed class UnpublishManuscriptCommandHandler : IRequestHandler<UnpublishManuscriptCommand>
{
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UnpublishManuscriptCommandHandler(
        IRepository<Manuscript> manuscripts,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _manuscripts = manuscripts;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(UnpublishManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.EnsureNotActingOnOwn(manuscript.AuthorId, _currentUser);
        ManuscriptAccess.ApplyTransition(manuscript.Unpublish);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
