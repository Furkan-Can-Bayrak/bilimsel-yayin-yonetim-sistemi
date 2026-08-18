using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.UnpublishManuscript;

public sealed record UnpublishManuscriptCommand(int Id) : IRequest;

public sealed class UnpublishManuscriptCommandHandler : IRequestHandler<UnpublishManuscriptCommand>
{
    private readonly IApplicationDbContext _db;

    public UnpublishManuscriptCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UnpublishManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        ManuscriptAccess.ApplyTransition(manuscript.Unpublish);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
