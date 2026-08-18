using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Reviews.Queries.GetReviewCandidates;

public sealed record ReviewerCandidateDto(int Id, string FirstName, string LastName, string Email);

public sealed record GetReviewCandidatesQuery(int ManuscriptId) : IRequest<IReadOnlyList<ReviewerCandidateDto>>;

public sealed class GetReviewCandidatesQueryHandler
    : IRequestHandler<GetReviewCandidatesQuery, IReadOnlyList<ReviewerCandidateDto>>
{
    private readonly IApplicationDbContext _db;

    public GetReviewCandidatesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ReviewerCandidateDto>> Handle(
        GetReviewCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var authorId = await _db.Manuscripts
            .Where(m => m.Id == request.ManuscriptId)
            .Select(m => (int?)m.AuthorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (authorId is null)
        {
            return [];
        }

        return await _db.Users
            .AsNoTracking()
            .Where(u => u.IsActive && u.Id != authorId)
            .Where(u => u.UserRoles.Any(ur =>
                ur.Role.RolePermissions.Any(rp => rp.Permission.Code == Permissions.Reviews.Submit)))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new ReviewerCandidateDto(u.Id, u.FirstName, u.LastName, u.Email))
            .ToListAsync(cancellationToken);
    }
}
