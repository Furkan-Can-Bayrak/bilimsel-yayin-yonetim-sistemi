using Blog.Application.Categories.Dtos;
using Blog.Application.Common;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Slug) : IRequest<CreateCategoryResult>;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly IApplicationDbContext _db;

    public CreateCategoryCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CreateCategoryResult> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var source = string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug!;
        var baseSlug = SlugHelper.GenerateFromTitle(source);
        if (baseSlug == "post")
        {
            baseSlug = "kategori";
        }

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.Categories.AnyAsync(c => c.Slug == s, cancellationToken),
            baseSlug,
            cancellationToken);

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult(category.Id, category.Slug);
    }
}
