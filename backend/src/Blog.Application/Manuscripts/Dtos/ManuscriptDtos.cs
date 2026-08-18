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
    bool IsPublished,
    int ResearchAreaId,
    string ResearchAreaName,
    int AuthorId,
    string AuthorName);

public sealed record AdminManuscriptDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    DateTime? PublishedAt,
    bool IsPublished,
    int ResearchAreaId,
    string ResearchAreaName,
    int AuthorId,
    string AuthorName);

public sealed record CreateManuscriptResult(int Id, string Slug);
