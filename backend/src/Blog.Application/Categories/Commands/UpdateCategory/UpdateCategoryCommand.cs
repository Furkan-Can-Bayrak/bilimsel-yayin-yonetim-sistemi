using Blog.Application.Common;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(int Id, string Name, string? Slug) : IRequest;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateCategoryCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Kategori bulunamadı: {request.Id}");
        }

        var source = string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug!;
        var baseSlug = SlugHelper.GenerateFromTitle(source);
        if (baseSlug == "post")
        {
            baseSlug = "kategori";
        }

        var slug = await SlugHelper.EnsureUniqueSlugAsync(
            s => _db.Categories.AnyAsync(c => c.Slug == s && c.Id != request.Id, cancellationToken),
            baseSlug,
            cancellationToken);

        category.Name = request.Name.Trim();
        category.Slug = slug;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
