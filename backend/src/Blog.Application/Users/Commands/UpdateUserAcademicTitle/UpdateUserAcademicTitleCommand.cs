using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly IApplicationDbContext _db;

    public UpdateUserAcademicTitleCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateUserAcademicTitleCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }

        if (user.AcademicTitle == request.AcademicTitle)
        {
            return;
        }

        user.AcademicTitle = request.AcademicTitle;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
