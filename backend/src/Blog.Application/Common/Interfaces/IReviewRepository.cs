using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByIdWithManuscriptAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> HasOpenForManuscriptAsync(
        int manuscriptId,
        CancellationToken cancellationToken = default);

    Task<bool> CanUserSubmitReviewsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
