using Blog.Application.Common.Interfaces;
using Blog.Application.Manuscripts.Dtos;
using Blog.Application.Manuscripts.Queries.GetAdminManuscripts;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Manuscripts.Queries.GetManuscriptById;

public sealed record GetManuscriptByIdQuery(int Id) : IRequest<AdminManuscriptDetailDto?>;

public sealed class GetManuscriptByIdQueryHandler
    : IRequestHandler<GetManuscriptByIdQuery, AdminManuscriptDetailDto?>
{
    private readonly IManuscriptRepository _manuscripts;
    private readonly IReviewRepository _reviews;
    private readonly ICurrentUser _currentUser;

    public GetManuscriptByIdQueryHandler(
        IManuscriptRepository manuscripts,
        IReviewRepository reviews,
        ICurrentUser currentUser)
    {
        _manuscripts = manuscripts;
        _reviews = reviews;
        _currentUser = currentUser;
    }

    public async Task<AdminManuscriptDetailDto?> Handle(
        GetManuscriptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var includeReview = _currentUser.HasPermission(Permissions.Reviews.ViewAll)
            || _currentUser.HasPermission(Permissions.Reviews.Submit);

        var manuscript = await _manuscripts.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (manuscript is null)
        {
            return null;
        }

        var isAssignedReviewer = _currentUser.UserId is int userId &&
            await _reviews.ExistsForManuscriptAndReviewerAsync(
                request.Id,
                userId,
                cancellationToken);

        if (!ManuscriptAccess.CanViewRecord(manuscript, _currentUser, isAssignedReviewer))
        {
            return null;
        }

        var dto = Map(manuscript, includeReview);
        return ApplyReviewVisibility(dto, manuscript);
    }

    private AdminManuscriptDetailDto ApplyReviewVisibility(
        AdminManuscriptDetailDto dto,
        Manuscript manuscript)
    {
        if (_currentUser.HasPermission(Permissions.Reviews.ViewAll))
        {
            return dto;
        }

        if (_currentUser.UserId == manuscript.AuthorId)
        {
            return dto with { CurrentReview = null, Reviews = [] };
        }

        var own = dto.Reviews
            .Where(r => r.ReviewerId == _currentUser.UserId)
            .ToList();

        return dto with
        {
            CurrentReview = own.FirstOrDefault(),
            Reviews = own
        };
    }

    private static AdminManuscriptDetailDto Map(Manuscript manuscript, bool includeReview)
    {
        var reviews = includeReview
            ? AdminManuscriptListMapping.MapReviews(manuscript)
            : [];

        return new AdminManuscriptDetailDto(
            manuscript.Id,
            manuscript.Title,
            manuscript.Slug,
            manuscript.Content,
            manuscript.Summary,
            manuscript.PublishedAt,
            manuscript.Status,
            manuscript.ResearchAreaId,
            manuscript.ResearchArea?.Name ?? string.Empty,
            manuscript.AuthorId,
            manuscript.Author is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    manuscript.Author.AcademicTitle,
                    manuscript.Author.FirstName,
                    manuscript.Author.LastName),
            reviews.FirstOrDefault(),
            reviews);
    }
}
