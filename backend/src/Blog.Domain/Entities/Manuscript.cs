using Blog.Domain.Common;

namespace Blog.Domain.Entities;

/// <summary>Bilimsel makale. Yayın durumu, yazarı ve araştırma alanı ile birlikte tutulur.</summary>
public sealed class Manuscript : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public int ResearchAreaId { get; set; }
    public ResearchArea? ResearchArea { get; set; }

    /// <summary>Makaleyi oluşturan kullanıcı. İçerik düzenleme bu kişiye (veya ViewAll sahibine) aittir.</summary>
    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    /// <returns>Yeni yayına alındıysa true; zaten yayındaysa false.</returns>
    public bool Publish(DateTime utcNow)
    {
        if (IsPublished)
        {
            return false;
        }

        IsPublished = true;
        PublishedAt = utcNow;
        return true;
    }

    /// <returns>Yayından alındıysa true; zaten taslaktaysa false.</returns>
    public bool Unpublish()
    {
        if (!IsPublished)
        {
            return false;
        }

        IsPublished = false;
        PublishedAt = null;
        return true;
    }
}
