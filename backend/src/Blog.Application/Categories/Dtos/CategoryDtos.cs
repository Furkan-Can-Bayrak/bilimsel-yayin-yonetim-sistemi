namespace Blog.Application.Categories.Dtos;

public sealed record CategoryDto(int Id, string Name, string Slug, int PostCount);

public sealed record CreateCategoryResult(int Id, string Slug);
