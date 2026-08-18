using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Manuscripts;

/// <summary>
/// ViewAll: tüm makaleler. Aksi halde kendi yazdığı veya hakem olarak atandığı kayıtlar.
/// </summary>
internal static class ManuscriptAccess
{
    public static bool CanViewAll(ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.ViewAll);

    public static bool CanView(int authorId, ICurrentUser user, bool isAssignedReviewer = false) =>
        CanViewAll(user) || user.UserId == authorId || isAssignedReviewer;

    public static bool CanUpdate(int authorId, ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.Update) &&
        (user.UserId == authorId || CanViewAll(user));

    public static bool CanEditContent(Manuscript manuscript, ICurrentUser user)
    {
        if (!CanUpdate(manuscript.AuthorId, user))
        {
            return false;
        }

        if (CanViewAll(user))
        {
            return true;
        }

        return manuscript.Status is ManuscriptStatus.Draft or ManuscriptStatus.Rejected;
    }

    public static IQueryable<Manuscript> VisibleTo(IQueryable<Manuscript> query, ICurrentUser user)
    {
        if (CanViewAll(user))
        {
            return query;
        }

        if (user.UserId is int userId)
        {
            return query.Where(m =>
                m.AuthorId == userId ||
                m.Reviews.Any(r => r.ReviewerId == userId));
        }

        return query.Where(_ => false);
    }

    public static void ApplyTransition(Action transition)
    {
        try
        {
            transition();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }
    }
}
