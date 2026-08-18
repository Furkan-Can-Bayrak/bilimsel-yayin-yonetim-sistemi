using Blog.Domain.Entities;
using Blog.Domain.Enums;

namespace Blog.Application.Tests.Manuscripts;

public class ManuscriptWorkflowTests
{
    [Fact]
    public void Submit_from_draft_sets_submitted()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Draft };

        manuscript.Submit();

        Assert.Equal(ManuscriptStatus.Submitted, manuscript.Status);
    }

    [Fact]
    public void Submit_from_rejected_sets_submitted()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Rejected };

        manuscript.Submit();

        Assert.Equal(ManuscriptStatus.Submitted, manuscript.Status);
    }

    [Fact]
    public void Publish_from_draft_throws()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Draft };

        Assert.Throws<InvalidOperationException>(() => manuscript.Publish(DateTime.UtcNow));
    }

    [Fact]
    public void Accept_then_publish_sets_published_at()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Submitted };
        var now = new DateTime(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

        manuscript.Accept();
        manuscript.Publish(now);

        Assert.Equal(ManuscriptStatus.Published, manuscript.Status);
        Assert.Equal(now, manuscript.PublishedAt);
    }

    [Fact]
    public void Unpublish_returns_to_accepted()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Submitted };
        manuscript.Accept();
        manuscript.Publish(DateTime.UtcNow);

        manuscript.Unpublish();

        Assert.Equal(ManuscriptStatus.Accepted, manuscript.Status);
        Assert.Null(manuscript.PublishedAt);
    }

    [Fact]
    public void AssignReviewer_from_submitted_sets_under_review()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Submitted };

        manuscript.AssignReviewer();

        Assert.Equal(ManuscriptStatus.UnderReview, manuscript.Status);
    }

    [Fact]
    public void Accept_from_under_review_succeeds()
    {
        var manuscript = new Manuscript { Status = ManuscriptStatus.Submitted };
        manuscript.AssignReviewer();

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
}
