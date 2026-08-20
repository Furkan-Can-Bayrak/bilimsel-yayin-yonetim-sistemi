namespace Blog.Domain.Entities;

/// <summary>Kullanıcıya özel uygulama içi bildirim.</summary>
public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int? RelatedManuscriptId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsRead { get; set; }
}
