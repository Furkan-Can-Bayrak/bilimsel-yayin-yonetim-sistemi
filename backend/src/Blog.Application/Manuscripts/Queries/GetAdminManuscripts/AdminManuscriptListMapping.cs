using Blog.Application.Manuscripts.Dtos;
using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Manuscripts.Queries.GetAdminManuscripts;

internal static class AdminManuscriptListMapping
{
    public static AdminManuscriptListItemDto ToListItem(Manuscript manuscript, bool includeReview)
    {
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
            includeReview ? MapCurrentReview(manuscript) : null);
    }

    private static ReviewSummaryDto? MapCurrentReview(Manuscript manuscript)
    {
        var review = manuscript.Reviews
            .OrderByDescending(r => r.AssignedAtUtc)
            .FirstOrDefault();

        if (review is null)
        {
            return null;
        }

        return new ReviewSummaryDto(
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
}
