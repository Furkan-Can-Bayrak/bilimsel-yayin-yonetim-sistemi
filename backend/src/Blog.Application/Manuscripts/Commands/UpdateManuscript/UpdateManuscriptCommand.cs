using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Commands.UpdateManuscript;

public sealed record UpdateManuscriptCommand(
    int Id,
    string Title,
    string Content,
    string? Summary,
    int ResearchAreaId) : IRequest;

public sealed class UpdateManuscriptCommandValidator : AbstractValidator<UpdateManuscriptCommand>
{
    public UpdateManuscriptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(500).When(x => x.Summary is not null);
        RuleFor(x => x.ResearchAreaId).GreaterThan(0);
    }
}

public sealed class UpdateManuscriptCommandHandler : IRequestHandler<UpdateManuscriptCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateManuscriptCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateManuscriptCommand request, CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (manuscript is null)
        {
            throw new NotFoundException($"Makale bulunamadı: {request.Id}");
        }

        if (!ManuscriptAccess.CanUpdate(manuscript.AuthorId, _currentUser))
        {
            throw new ForbiddenException("Yalnızca kendi makalenizi düzenleyebilirsiniz.");
        }

        if (!ManuscriptAccess.CanEditContent(manuscript, _currentUser))
        {
            throw new ConflictException("Bu durumdayken makale düzenlenemez.");
        }

        var areaExists = await _db.ResearchAreas
            .AnyAsync(a => a.Id == request.ResearchAreaId, cancellationToken);

        if (!areaExists)
        {
            throw new NotFoundException($"Araştırma alanı bulunamadı: {request.ResearchAreaId}");
        }

        var slug = await SlugHelper.GenerateUniqueSlugAsync(
            request.Title,
            nameof(request.Title),
            s => _db.Manuscripts.AnyAsync(m => m.Slug == s && m.Id != request.Id, cancellationToken),
            cancellationToken);

        manuscript.Title = request.Title.Trim();
        manuscript.Content = request.Content;
        manuscript.Summary = request.Summary;
        manuscript.ResearchAreaId = request.ResearchAreaId;
        manuscript.Slug = slug;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
