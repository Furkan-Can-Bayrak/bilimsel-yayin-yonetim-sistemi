namespace Blog.Application.Common.Models;

/// <summary>
/// Persistence read model — API DTO değil. Makale sayısı SQL'de üretilir.
/// </summary>
public sealed record ResearchAreaWithCount(int Id, string Name, string Slug, int ManuscriptCount);
