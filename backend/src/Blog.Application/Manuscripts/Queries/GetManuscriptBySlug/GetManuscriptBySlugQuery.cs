using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptBySlug;

public sealed record GetManuscriptBySlugQuery(string Slug) : IRequest<ManuscriptDetailDto?>;

public sealed class GetManuscriptBySlugQueryHandler
    : IRequestHandler<GetManuscriptBySlugQuery, ManuscriptDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetManuscriptBySlugQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ManuscriptDetailDto?> Handle(
        GetManuscriptBySlugQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.Status == ManuscriptStatus.Published && m.Slug == request.Slug)
            .Select(m => new ManuscriptDetailDto(
                m.Id,
                m.Title,
                m.Slug,
                m.Content,
                m.Summary,
                m.PublishedAt,
                m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                m.ResearchArea != null ? m.ResearchArea.Slug : string.Empty,
                m.Author == null
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(m.Author.AcademicTitle)
                        ? m.Author.FirstName + " " + m.Author.LastName
                        : m.Author.AcademicTitle + " " + m.Author.FirstName + " " + m.Author.LastName))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
