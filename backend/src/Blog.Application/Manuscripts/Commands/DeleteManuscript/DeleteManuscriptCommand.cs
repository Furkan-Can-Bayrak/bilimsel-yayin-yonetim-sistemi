using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.DeleteManuscript;

public sealed record DeleteManuscriptCommand(int Id) : IRequest;

public sealed class DeleteManuscriptCommandValidator : AbstractValidator<DeleteManuscriptCommand>
{
    public DeleteManuscriptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public sealed class DeleteManuscriptCommandHandler : IRequestHandler<DeleteManuscriptCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteManuscriptCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        _db.Manuscripts.Remove(manuscript);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
