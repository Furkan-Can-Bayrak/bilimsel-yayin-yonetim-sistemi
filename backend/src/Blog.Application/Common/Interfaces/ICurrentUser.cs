namespace Blog.Application.Common.Interfaces;

/// <summary>İstek yapan kullanıcının kimliği ve izinleri. HTTP'den okunur; handler'lar HttpContext görmez.</summary>
public interface ICurrentUser
{
    int? UserId { get; }

    bool HasPermission(string permission);

    /// <summary>Kimliği yoksa oturum bozuk demektir.</summary>
    int RequireUserId();
}
