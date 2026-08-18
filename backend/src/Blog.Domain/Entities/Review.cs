using Blog.Domain.Enums;

namespace Blog.Domain.Entities;

/// <summary>Bir makaleye tek hakem ataması. Teslim edilene kadar açık kabul edilir.</summary>
public sealed class Review
{
    public int Id { get; set; }
    public int ManuscriptId { get; set; }
    public Manuscript? Manuscript { get; set; }
    public int ReviewerId { get; set; }
    public User? Reviewer { get; set; }

    public ReviewRecommendation? Recommendation { get; set; }
    public string? Comments { get; set; }

    public DateTime AssignedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }

    public bool IsSubmitted => SubmittedAtUtc is not null;

    public void SubmitReport(ReviewRecommendation recommendation, string comments, DateTime utcNow)
    {
        if (IsSubmitted)
        {
            throw new InvalidOperationException("Bu değerlendirme zaten teslim edilmiş.");
        }

        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new InvalidOperationException("Gerekçe zorunludur.");
        }

        Recommendation = recommendation;
        Comments = comments.Trim();
        SubmittedAtUtc = utcNow;
    }
}
