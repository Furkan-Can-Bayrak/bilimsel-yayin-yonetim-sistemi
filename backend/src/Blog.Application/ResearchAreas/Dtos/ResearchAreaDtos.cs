namespace Blog.Application.ResearchAreas.Dtos;

public sealed record ResearchAreaDto(int Id, string Name, string Slug, int ManuscriptCount);

public sealed record CreateResearchAreaResult(int Id, string Slug);
