using Blog.Domain.Common;

namespace Blog.Domain.Entities;

/// <summary>Makalelerin sınıflandığı araştırma alanı (ör. Yapay Zeka, Malzeme Bilimi).</summary>
public sealed class ResearchArea : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<Manuscript> Manuscripts { get; set; } = new List<Manuscript>();
}
