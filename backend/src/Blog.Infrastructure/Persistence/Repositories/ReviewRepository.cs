using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(BlogDbContext db)
        : base(db)
    {
    }

    public Task<Review?> GetByIdWithManuscriptAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Set.Include(r => r.Manuscript)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> HasOpenForManuscriptAsync(
        int manuscriptId,
        CancellationToken cancellationToken = default) =>
        Set.AnyAsync(
            r => r.ManuscriptId == manuscriptId && r.SubmittedAtUtc == null,
            cancellationToken);
}
