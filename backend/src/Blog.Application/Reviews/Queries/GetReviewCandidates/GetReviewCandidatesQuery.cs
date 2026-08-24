using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using MediatR;

namespace Blog.Application.Reviews.Queries.GetReviewCandidates;

public sealed record ReviewerCandidateDto(int Id, string FirstName, string LastName, string Email);

public sealed record GetReviewCandidatesQuery(int ManuscriptId) : IRequest<IReadOnlyList<ReviewerCandidateDto>>;

public sealed class GetReviewCandidatesQueryHandler
    : IRequestHandler<GetReviewCandidatesQuery, IReadOnlyList<ReviewerCandidateDto>>
{
    private readonly IManuscriptRepository _manuscripts;
    private readonly IUserRepository _users;

    public GetReviewCandidatesQueryHandler(
        IManuscriptRepository manuscripts,
        IUserRepository users)
    {
        _manuscripts = manuscripts;
        _users = users;
    }

    public async Task<IReadOnlyList<ReviewerCandidateDto>> Handle(
        GetReviewCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var manuscript = await _manuscripts.GetByIdAsync(request.ManuscriptId, cancellationToken);

        if (manuscript is null)
        {
            return [];
        }

        var candidates = await _users.ListActiveByPermissionAsync(
            Permissions.Reviews.Submit,
            manuscript.AuthorId,
            cancellationToken);

        return candidates
            .Select(u => new ReviewerCandidateDto(u.Id, u.FirstName, u.LastName, u.Email))
            .ToList();
    }
}
