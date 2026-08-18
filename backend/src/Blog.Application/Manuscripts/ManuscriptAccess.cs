using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;

namespace Blog.Application.Manuscripts;

/// <summary>
/// ViewAll: tüm makaleler. Aksi halde yalnızca kendi yazdığı kayıtlar.
/// Güncelleme: Update izni + (kendi makalesi veya ViewAll).
/// </summary>
internal static class ManuscriptAccess
{
    public static bool CanViewAll(ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.ViewAll);

    public static bool CanView(int authorId, ICurrentUser user) =>
        CanViewAll(user) || user.UserId == authorId;

    public static bool CanUpdate(int authorId, ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.Update) &&
        (user.UserId == authorId || CanViewAll(user));

    public static IQueryable<Manuscript> VisibleTo(IQueryable<Manuscript> query, ICurrentUser user)
    {
        if (CanViewAll(user))
        {
            return query;
        }

        if (user.UserId is int userId)
        {
            return query.Where(m => m.AuthorId == userId);
        }

        return query.Where(_ => false);
    }
}
