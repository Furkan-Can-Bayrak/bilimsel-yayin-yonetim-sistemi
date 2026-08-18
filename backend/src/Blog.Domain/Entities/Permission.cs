namespace Blog.Domain.Entities;

/// <summary>
/// Tek bir yetki birimi. Kayıtlar <see cref="Authorization.Permissions"/> içindeki
/// sabitlerden seed edilir; panelden eklenip silinmez.
/// </summary>
public sealed class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
