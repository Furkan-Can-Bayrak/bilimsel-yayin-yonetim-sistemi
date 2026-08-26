using Blog.Domain.Enums;

namespace Blog.Application.Manuscripts.Dtos;

public sealed record ManuscriptListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Summary,
    DateTime? PublishedAt,
    string ResearchAreaName,
    string AuthorName);

public sealed record ManuscriptDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    DateTime? PublishedAt,
    string ResearchAreaName,
    string ResearchAreaSlug,
    string AuthorName);

public sealed record AdminManuscriptListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Summary,
    DateTime? PublishedAt,
    ManuscriptStatus Status,
    int? ResearchAreaId,
    string ResearchAreaName,
    int AuthorId,
    string AuthorName,
    ReviewSummaryDto? CurrentReview,
    IReadOnlyList<ReviewSummaryDto> Reviews);

public sealed record AdminManuscriptDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    DateTime? PublishedAt,
    ManuscriptStatus Status,
    int? ResearchAreaId,
    string ResearchAreaName,
    int AuthorId,
    string AuthorName,
    ReviewSummaryDto? CurrentReview,
    IReadOnlyList<ReviewSummaryDto> Reviews);

public sealed record ReviewSummaryDto(
    int Id,
    int ReviewerId,
    string ReviewerName,
    DateTime AssignedAtUtc,
    DateTime? SubmittedAtUtc,
    ReviewRecommendation? Recommendation,
    string? Comments);

public sealed record CreateManuscriptResult(int Id, string Slug);
