using Blog.Domain.Common;

namespace Blog.Domain.Entities;

/// <summary>
/// İzin demeti. Veritabanı kaydı olduğu için yeni rol eklemek deploy gerektirmez.
/// </summary>
public sealed class Role : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Sistemin çalışması için gereken rol; panelden silinemez (ör. Admin).</summary>
    public bool IsSystemRole { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
