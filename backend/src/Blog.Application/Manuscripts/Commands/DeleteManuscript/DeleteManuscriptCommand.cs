using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;

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
    private readonly IRepository<Manuscript> _manuscripts;
    private readonly IUnitOfWork _uow;

    public DeleteManuscriptCommandHandler(IRepository<Manuscript> manuscripts, IUnitOfWork uow)
    {
        _manuscripts = manuscripts;
        _uow = uow;
    }

    public async Task Handle(DeleteManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        _manuscripts.Remove(manuscript);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
