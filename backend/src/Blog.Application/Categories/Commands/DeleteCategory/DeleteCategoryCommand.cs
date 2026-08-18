using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(int Id) : IRequest;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteCategoryCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Kategori bulunamadı: {request.Id}");
        }

        var hasPosts = await _db.Posts.AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
        if (hasPosts)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Id"] = ["Bu kategoriye bağlı yazılar var; önce yazıları silin veya taşıyın."]
            });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
