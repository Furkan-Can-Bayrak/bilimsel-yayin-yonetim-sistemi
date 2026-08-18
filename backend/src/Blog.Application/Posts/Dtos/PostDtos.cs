namespace Blog.Application.Posts.Dtos;

/// <summary>
/// Liste için hafif DTO — entity'nin tüm alanlarını dışarı açmıyoruz.
/// </summary>
public sealed record PostListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Summary,
    DateTime? PublishedAt,
    string CategoryName);

/// <summary>
/// Detay sayfası için DTO — Content dahil.
/// </summary>
public sealed record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    DateTime? PublishedAt,
    string CategoryName,
    string CategorySlug);

/// <summary>Admin listesi — taslaklar dahil.</summary>
public sealed record AdminPostListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Summary,
    DateTime? PublishedAt,
    bool IsPublished,
    int CategoryId,
    string CategoryName);

/// <summary>Admin düzenleme formu için.</summary>
public sealed record AdminPostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    DateTime? PublishedAt,
    bool IsPublished,
    int CategoryId,
    string CategoryName);
