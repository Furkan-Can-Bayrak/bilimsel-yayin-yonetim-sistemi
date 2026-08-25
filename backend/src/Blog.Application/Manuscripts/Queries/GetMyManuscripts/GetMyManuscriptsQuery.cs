using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Application.Manuscripts.Dtos;
using Blog.Application.Manuscripts.Queries.GetAdminManuscripts;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Manuscripts.Queries.GetMyManuscripts;

public sealed record GetMyManuscriptsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    int? ResearchAreaId = null,
    ManuscriptStatus? Status = null) : IRequest<PagedResult<AdminManuscriptListItemDto>>;

public sealed class GetMyManuscriptsQueryHandler
    : IRequestHandler<GetMyManuscriptsQuery, PagedResult<AdminManuscriptListItemDto>>
{
    private const int MaxPageSize = 50;
    private readonly IManuscriptRepository _manuscripts;
    private readonly ICurrentUser _currentUser;

    public GetMyManuscriptsQueryHandler(
        IManuscriptRepository manuscripts,
        ICurrentUser currentUser)
    {
        _manuscripts = manuscripts;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AdminManuscriptListItemDto>> Handle(
        GetMyManuscriptsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 10
            : Math.Min(request.PageSize, MaxPageSize);

        var authorId = _currentUser.RequireUserId();

        var (manuscripts, totalCount) = await _manuscripts.ListMinePagedAsync(
            page,
            pageSize,
            request.Search,
            request.ResearchAreaId,
            request.Status,
            authorId,
            cancellationToken);

        var items = manuscripts
            .Select(m => AdminManuscriptListMapping.ToListItem(m, includeReview: false))
            .ToList();

        return new PagedResult<AdminManuscriptListItemDto>(items, page, pageSize, totalCount);
    }
}
