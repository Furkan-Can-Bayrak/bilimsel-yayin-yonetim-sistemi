using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly IApplicationDbContext _db;

    public DeleteResearchAreaCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteResearchAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _db.ResearchAreas
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (area is null)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.Id}");
        }

        var hasManuscripts = await _db.Manuscripts
            .AnyAsync(m => m.ResearchAreaId == request.Id, cancellationToken);

        if (hasManuscripts)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Id"] = ["Bu alana bağlı makaleler var; önce makaleleri silin veya taşıyın."]
            });
        }

        _db.ResearchAreas.Remove(area);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
