namespace Blog.Domain.Entities;

/// <summary>
/// Basit admin kullanıcı — ASP.NET Identity tabloları yerine tek tablo (öğrenme için).
/// </summary>
public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
}
