using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptById;

public sealed record GetManuscriptByIdQuery(int Id) : IRequest<AdminManuscriptDetailDto?>;

public sealed class GetManuscriptByIdQueryHandler
    : IRequestHandler<GetManuscriptByIdQuery, AdminManuscriptDetailDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetManuscriptByIdQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AdminManuscriptDetailDto?> Handle(
        GetManuscriptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var manuscript = await _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new AdminManuscriptDetailDto(
                m.Id,
                m.Title,
                m.Slug,
                m.Content,
                m.Summary,
                m.PublishedAt,
                m.IsPublished,
                m.ResearchAreaId,
                m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.AuthorId,
                m.Author == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(m.Author.AcademicTitle)
                        ? m.Author.FullName
                        : m.Author.AcademicTitle + " " + m.Author.FullName))
            .FirstOrDefaultAsync(cancellationToken);

        if (manuscript is null || !ManuscriptAccess.CanView(manuscript.AuthorId, _currentUser))
        {
            return null;
        }

        return manuscript;
    }
}
