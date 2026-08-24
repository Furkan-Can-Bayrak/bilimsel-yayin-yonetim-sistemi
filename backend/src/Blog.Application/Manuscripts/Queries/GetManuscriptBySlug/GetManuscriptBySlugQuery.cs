using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptBySlug;

public sealed record GetManuscriptBySlugQuery(string Slug) : IRequest<ManuscriptDetailDto?>;

public sealed class GetManuscriptBySlugQueryHandler
    : IRequestHandler<GetManuscriptBySlugQuery, ManuscriptDetailDto?>
{
    private readonly IManuscriptRepository _manuscripts;

    public GetManuscriptBySlugQueryHandler(IManuscriptRepository manuscripts)
    {
        _manuscripts = manuscripts;
    }

    public async Task<ManuscriptDetailDto?> Handle(
        GetManuscriptBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetPublishedBySlugAsync(request.Slug, cancellationToken);

        if (manuscript is null)
        {
            return null;
        }

        return new ManuscriptDetailDto(
            manuscript.Id,
            manuscript.Title,
            manuscript.Slug,
            manuscript.Content,
            manuscript.Summary,
            manuscript.PublishedAt,
            manuscript.ResearchArea?.Name ?? string.Empty,
            manuscript.ResearchArea?.Slug ?? string.Empty,
            manuscript.Author is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    manuscript.Author.AcademicTitle,
                    manuscript.Author.FirstName,
                    manuscript.Author.LastName));
    }
}
