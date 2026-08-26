using Blog.Domain.Common;
using Blog.Domain.Enums;

namespace Blog.Domain.Entities;

/// <summary>Bilimsel makale. Durum makinesi, yazarı ve araştırma alanı ile birlikte tutulur.</summary>
public sealed class Manuscript : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime? PublishedAt { get; private set; }
    public ManuscriptStatus Status { get; private set; } = ManuscriptStatus.Draft;
    /// <summary>Taslakta boş kalabilir; değerlendirmeye göndermeden önce zorunlu.</summary>
    public int? ResearchAreaId { get; set; }
    public ResearchArea? ResearchArea { get; set; }

    /// <summary>Makaleyi oluşturan kullanıcı. İçerik düzenleme bu kişiye (veya ViewAll sahibine) aittir.</summary>
    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public DateTime? DeletedAtUtc { get; set; }

    public void Submit()
    {
        if (Status is not (ManuscriptStatus.Draft or ManuscriptStatus.Rejected))
        {
            throw new InvalidOperationException(
                "Yalnızca taslak veya reddedilmiş makale değerlendirmeye gönderilebilir.");
        }

        Status = ManuscriptStatus.Submitted;
    }

    public void AssignReviewer()
    {
        if (Status is not (ManuscriptStatus.Submitted or ManuscriptStatus.UnderReview))
        {
            throw new InvalidOperationException(
                "Hakem yalnızca gönderilmiş veya incelemedeki makaleye atanabilir.");
        }

        Status = ManuscriptStatus.UnderReview;
    }

    public void ReturnToSubmitted()
    {
        if (Status != ManuscriptStatus.UnderReview)
        {
            throw new InvalidOperationException(
                "Yalnızca incelemedeki makale gönderildi durumuna alınabilir.");
        }

        Status = ManuscriptStatus.Submitted;
    }

    public void Accept()
    {
        if (Status is not (ManuscriptStatus.Submitted or ManuscriptStatus.UnderReview))
        {
            throw new InvalidOperationException("Yalnızca gönderilmiş veya incelemedeki makale kabul edilebilir.");
        }

        Status = ManuscriptStatus.Accepted;
    }

    public void Reject()
    {
        if (Status is not (ManuscriptStatus.Submitted or ManuscriptStatus.UnderReview))
        {
            throw new InvalidOperationException("Yalnızca gönderilmiş veya incelemedeki makale reddedilebilir.");
        }

        Status = ManuscriptStatus.Rejected;
    }

    public void Publish(DateTime utcNow)
    {
        if (Status != ManuscriptStatus.Accepted)
        {
            throw new InvalidOperationException("Yalnızca kabul edilmiş makale yayınlanabilir.");
        }

        Status = ManuscriptStatus.Published;
        PublishedAt = utcNow;
    }

    public void Unpublish()
    {
        if (Status != ManuscriptStatus.Published)
        {
            throw new InvalidOperationException("Yalnızca yayındaki makale yayından alınabilir.");
        }

        Status = ManuscriptStatus.Accepted;
        PublishedAt = null;
    }
}
