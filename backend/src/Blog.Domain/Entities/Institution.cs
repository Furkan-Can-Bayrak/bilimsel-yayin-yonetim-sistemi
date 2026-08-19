using Blog.Domain.Common;

namespace Blog.Domain.Entities;

/// <summary>Yazar ve hakemin bağlı olduğu kurum (üniversite, enstitü).</summary>
public sealed class Institution : ISoftDeletable
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Kısa ad, ör. "ODTÜ", "İTÜ". Yoksa null.</summary>
    public string? Abbreviation { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
