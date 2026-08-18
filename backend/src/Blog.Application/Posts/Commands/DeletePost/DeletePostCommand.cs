using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Posts.Commands.DeletePost;

public sealed record DeletePostCommand(int Id) : IRequest;

public sealed class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IApplicationDbContext _db;

    public DeletePostCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Yazı bulunamadı: {request.Id}");
        }

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
