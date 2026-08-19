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
        var row = await _db.Manuscripts
            .AsNoTracking()
            .Where(m => m.Status == ManuscriptStatus.Published && m.Slug == request.Slug)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.Slug,
                m.Content,
                m.Summary,
                m.PublishedAt,
                ResearchAreaName = m.ResearchArea != null ? m.ResearchArea.Name : string.Empty,
                ResearchAreaSlug = m.ResearchArea != null ? m.ResearchArea.Slug : string.Empty,
                AuthorTitle = m.Author == null ? AcademicTitle.Dr : m.Author.AcademicTitle,
                AuthorFirstName = m.Author == null ? string.Empty : m.Author.FirstName,
                AuthorLastName = m.Author == null ? string.Empty : m.Author.LastName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new ManuscriptDetailDto(
            row.Id,
            row.Title,
            row.Slug,
            row.Content,
            row.Summary,
            row.PublishedAt,
            row.ResearchAreaName,
            row.ResearchAreaSlug,
            AcademicTitles.FormatName(row.AuthorTitle, row.AuthorFirstName, row.AuthorLastName));
    }
}
