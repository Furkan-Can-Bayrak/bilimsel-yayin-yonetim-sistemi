using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Manuscripts;

/// <summary>
/// ViewAll: tüm makaleler. Aksi halde yalnızca kendi yazdığı kayıtlar
/// (hakem atamaları Değerlendirmelerim kuyruğundadır).
/// </summary>
internal static class ManuscriptAccess
{
    public static bool CanViewAll(ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.ViewAll);

    public static bool CanView(int authorId, ICurrentUser user, bool isAssignedReviewer = false) =>
        CanViewAll(user) || user.UserId == authorId || isAssignedReviewer;

    /// <summary>Başkasının taslağı editöre de görünmez; yazar kendi taslağını görür.</summary>
    public static bool CanViewRecord(Manuscript manuscript, ICurrentUser user, bool isAssignedReviewer = false)
    {
        if (manuscript.Status == ManuscriptStatus.Draft && user.UserId != manuscript.AuthorId)
        {
            return false;
        }

        return CanView(manuscript.AuthorId, user, isAssignedReviewer);
    }

    /// <summary>Editör kendi makalesinde kabul/ret/hakem/yayın yapamaz.</summary>
    public static void EnsureNotActingOnOwn(int authorId, ICurrentUser user)
    {
        if (user.UserId == authorId)
        {
            throw new ForbiddenException("Kendi makaleniz üzerinde editör işlemi yapamazsınız.");
        }
    }

    /// <summary>Kim: Update izni ve makalenin yazarı. ViewAll düzenleme hakkı vermez.</summary>
    public static bool CanUpdate(int authorId, ICurrentUser user) =>
        user.HasPermission(Permissions.Manuscripts.Update) &&
        user.UserId == authorId;

    /// <summary>
    /// Ne zaman: yalnız taslak veya ret. Kim olduğu <see cref="CanUpdate"/> ile kontrol edilir.
    /// </summary>
    public static bool CanEditContent(Manuscript manuscript, ICurrentUser user) =>
        manuscript.Status is ManuscriptStatus.Draft or ManuscriptStatus.Rejected;

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

    /// <summary>Açık hakem ataması varken kabul/ret yok; rapor beklenir veya atama geri alınır.</summary>
    public static void EnsureNoOpenReview(bool hasOpenReview)
    {
        if (hasOpenReview)
        {
            throw new ConflictException(
                "Hakem incelemesi sürerken makale kabul veya reddedilemez.");
        }
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
