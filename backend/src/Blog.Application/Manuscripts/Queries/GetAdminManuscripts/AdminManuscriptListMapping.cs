using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Manuscripts.Queries.GetAdminManuscripts;

internal static class AdminManuscriptListMapping
{
    public static AdminManuscriptListItemDto ToListItem(Manuscript manuscript, bool includeReview)
    {
        var reviews = includeReview ? MapReviews(manuscript) : [];

        return new AdminManuscriptListItemDto(
            manuscript.Id,
            manuscript.Title,
            manuscript.Slug,
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
            reviews,
            manuscript.RejectionReason);
    }

    public static IReadOnlyList<ReviewSummaryDto> MapReviews(Manuscript manuscript) =>
        manuscript.Reviews
            .OrderByDescending(r => r.AssignedAtUtc)
            .Select(MapReview)
            .ToList();

    public static ReviewSummaryDto MapReview(Review review) =>
        new(
            review.Id,
            review.ReviewerId,
            review.Reviewer is null
                ? string.Empty
                : AcademicTitles.FormatName(
                    review.Reviewer.AcademicTitle,
                    review.Reviewer.FirstName,
                    review.Reviewer.LastName),
            review.AssignedAtUtc,
            review.SubmittedAtUtc,
            review.Recommendation,
            review.Comments);
}
