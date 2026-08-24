using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
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

    public Task<bool> CanUserSubmitReviewsAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        Db.Users
            .Where(u => u.Id == userId)
            .AnyAsync(
                u => u.UserRoles.Any(ur =>
                    ur.Role.RolePermissions.Any(rp =>
                        rp.Permission.Code == Permissions.Reviews.Submit)),
                cancellationToken);
}
