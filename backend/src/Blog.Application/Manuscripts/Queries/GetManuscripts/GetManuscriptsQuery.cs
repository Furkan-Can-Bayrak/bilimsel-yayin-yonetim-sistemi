using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Manuscripts.Queries.GetManuscripts;

public sealed record GetManuscriptsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? ResearchAreaId = null) : IRequest<PagedResult<ManuscriptListItemDto>>;

public sealed class GetManuscriptsQueryHandler
    : IRequestHandler<GetManuscriptsQuery, PagedResult<ManuscriptListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IManuscriptRepository _manuscripts;

    public GetManuscriptsQueryHandler(IManuscriptRepository manuscripts)
    {
        _manuscripts = manuscripts;
    }

    public async Task<PagedResult<ManuscriptListItemDto>> Handle(
        GetManuscriptsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var (items, totalCount) = await _manuscripts.ListPublishedPagedAsync(
            page,
            pageSize,
            request.Search,
            request.ResearchAreaId,
            cancellationToken);

        var dtos = items.Select(m => new ManuscriptListItemDto(
            m.Id,
            m.Title,
            m.Slug,
            m.Summary,
            m.PublishedAt,
            m.ResearchArea?.Name ?? string.Empty,
            FormatAuthor(m.Author))).ToList();

        return new PagedResult<ManuscriptListItemDto>(dtos, page, pageSize, totalCount);
    }

    private static string FormatAuthor(User? author) =>
        author is null
            ? string.Empty
            : AcademicTitles.FormatName(author.AcademicTitle, author.FirstName, author.LastName);
}
