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

    public async Task<IReadOnlyList<Review>> ListByReviewerAsync(
        int reviewerId,
        CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking()
            .Include(r => r.Manuscript)
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.AssignedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Review?> GetByIdWithManuscriptAndReviewerAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(r => r.Manuscript)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}
