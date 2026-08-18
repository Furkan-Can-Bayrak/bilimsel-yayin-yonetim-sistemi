namespace Blog.Domain.Entities;

/// <summary>Role atanmış izin. Panelden eklenip çıkarılır.</summary>
public sealed class RolePermission
{
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
