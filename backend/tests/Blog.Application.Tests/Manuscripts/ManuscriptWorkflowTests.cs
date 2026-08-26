using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Tests.Manuscripts;

public class ManuscriptWorkflowTests
{
    [Fact]
    public void Submit_from_draft_sets_submitted()
    {
        var manuscript = new Manuscript();

        manuscript.Submit();

        Assert.Equal(ManuscriptStatus.Submitted, manuscript.Status);
    }

    [Fact]
    public void Submit_from_rejected_sets_submitted()
    {
        var manuscript = Submitted();
        manuscript.Reject("Yöntem eksik.");

        manuscript.Submit();

        Assert.Equal(ManuscriptStatus.Submitted, manuscript.Status);
        Assert.Null(manuscript.RejectionReason);
    }

    [Fact]
    public void Reject_sets_reason()
    {
        var manuscript = Submitted();

        manuscript.Reject("Kaynaklar yetersiz.");

        Assert.Equal(ManuscriptStatus.Rejected, manuscript.Status);
        Assert.Equal("Kaynaklar yetersiz.", manuscript.RejectionReason);
    }

    [Fact]
    public void Reject_without_reason_throws()
    {
        Assert.Throws<InvalidOperationException>(() => Submitted().Reject("  "));
    }

    [Fact]
    public void Publish_from_draft_throws()
    {
        var manuscript = new Manuscript();

        Assert.Throws<InvalidOperationException>(() => manuscript.Publish(DateTime.UtcNow));
    }

    [Fact]
    public void Accept_then_publish_sets_published_at()
    {
        var manuscript = Submitted();
        var now = new DateTime(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

        manuscript.Accept();
        manuscript.Publish(now);

        Assert.Equal(ManuscriptStatus.Published, manuscript.Status);
        Assert.Equal(now, manuscript.PublishedAt);
    }

    [Fact]
    public void Unpublish_returns_to_accepted()
    {
        var manuscript = Submitted();
        manuscript.Accept();
        manuscript.Publish(DateTime.UtcNow);

        manuscript.Unpublish();

        Assert.Equal(ManuscriptStatus.Accepted, manuscript.Status);
        Assert.Null(manuscript.PublishedAt);
    }

    [Fact]
    public void AssignReviewer_from_submitted_sets_under_review()
    {
        var manuscript = Submitted();

        manuscript.AssignReviewer();

        Assert.Equal(ManuscriptStatus.UnderReview, manuscript.Status);
    }

    [Fact]
    public void ReturnToSubmitted_from_under_review_sets_submitted()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();

        manuscript.ReturnToSubmitted();

        Assert.Equal(ManuscriptStatus.Submitted, manuscript.Status);
    }

    [Fact]
    public void ReturnToSubmitted_from_submitted_throws()
    {
        Assert.Throws<InvalidOperationException>(() => Submitted().ReturnToSubmitted());
    }

    [Fact]
    public void AssignReviewer_from_under_review_stays_under_review()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();

        manuscript.AssignReviewer();

        Assert.Equal(ManuscriptStatus.UnderReview, manuscript.Status);
    }

    [Fact]
    public void AssignReviewer_from_accepted_throws()
    {
        var manuscript = Submitted();
        manuscript.Accept();

        Assert.Throws<InvalidOperationException>(() => manuscript.AssignReviewer());
    }

    [Fact]
    public void AssignReviewer_from_draft_throws()
    {
        Assert.Throws<InvalidOperationException>(() => new Manuscript().AssignReviewer());
    }

    [Fact]
    public void Accept_from_under_review_succeeds()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();

        manuscript.Accept();

        Assert.Equal(ManuscriptStatus.Accepted, manuscript.Status);
    }

    [Fact]
    public void Accept_with_open_review_throws()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();
        manuscript.Reviews.Add(new Review { AssignedAtUtc = DateTime.UtcNow });

        Assert.Throws<InvalidOperationException>(() => manuscript.Accept());
    }

    [Fact]
    public void Reject_with_open_review_throws()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();
        manuscript.Reviews.Add(new Review { AssignedAtUtc = DateTime.UtcNow });

        Assert.Throws<InvalidOperationException>(() => manuscript.Reject("Kapsam dar."));
    }

    [Fact]
    public void Accept_after_review_submitted_succeeds()
    {
        var manuscript = Submitted();
        manuscript.AssignReviewer();
        var review = new Review { AssignedAtUtc = DateTime.UtcNow };
        review.SubmitReport(ReviewRecommendation.Accept, "Uygun.", DateTime.UtcNow);
        manuscript.Reviews.Add(review);

        manuscript.Accept();

        Assert.Equal(ManuscriptStatus.Accepted, manuscript.Status);
    }

    [Fact]
    public void Review_cannot_be_submitted_twice()
    {
        var review = new Review { AssignedAtUtc = DateTime.UtcNow };
        review.SubmitReport(ReviewRecommendation.Accept, "Uygun.", DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(
            () => review.SubmitReport(ReviewRecommendation.Reject, "Hayır.", DateTime.UtcNow));
    }

    [Fact]
    public void Review_submit_requires_comments()
    {
        var review = new Review { AssignedAtUtc = DateTime.UtcNow };

        Assert.Throws<InvalidOperationException>(
            () => review.SubmitReport(ReviewRecommendation.Accept, "  ", DateTime.UtcNow));
    }

    private static Manuscript Submitted()
    {
        var manuscript = new Manuscript();
        manuscript.Submit();
        return manuscript;
    }
}
