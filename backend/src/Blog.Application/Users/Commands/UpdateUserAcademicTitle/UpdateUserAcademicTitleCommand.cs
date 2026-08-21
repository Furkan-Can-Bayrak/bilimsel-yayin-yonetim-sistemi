using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Blog.Application.Users.Commands.UpdateUserAcademicTitle;

public sealed record UpdateUserAcademicTitleCommand(int UserId, AcademicTitle AcademicTitle) : IRequest;

public sealed class UpdateUserAcademicTitleCommandValidator : AbstractValidator<UpdateUserAcademicTitleCommand>
{
    private static readonly HashSet<AcademicTitle> AllowedTitles =
    [
        AcademicTitle.ProfDr,
        AcademicTitle.DocDr,
        AcademicTitle.DrOgrUyesi,
        AcademicTitle.OgrGor,
        AcademicTitle.ArsGor,
        AcademicTitle.Dr
    ];

    public UpdateUserAcademicTitleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AcademicTitle)
            .Must(AllowedTitles.Contains)
            .WithMessage("Geçerli bir akademik unvan seçin.");
    }
}

public sealed class UpdateUserAcademicTitleCommandHandler
    : IRequestHandler<UpdateUserAcademicTitleCommand>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _uow;

    public UpdateUserAcademicTitleCommandHandler(IRepository<User> users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task Handle(UpdateUserAcademicTitleCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }

        if (user.AcademicTitle == request.AcademicTitle)
        {
            return;
        }

        user.AcademicTitle = request.AcademicTitle;
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
