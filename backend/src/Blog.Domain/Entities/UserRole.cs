namespace Blog.Domain.Entities;

/// <summary>Kullanıcı ile rol arasındaki bağ. Bir kullanıcının birden fazla rolü olabilir.</summary>
public sealed class UserRole
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }
}
