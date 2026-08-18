using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comments)
            .HasMaxLength(4000);

        builder.Property(r => r.Recommendation)
            .HasConversion<int?>();

        builder.HasOne(r => r.Manuscript)
            .WithMany(m => m.Reviews)
            .HasForeignKey(r => r.ManuscriptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.ManuscriptId);

        // Aynı makalede aynı anda tek açık (teslim edilmemiş) atama.
        builder.HasIndex(r => r.ManuscriptId)
            .IsUnique()
            .HasFilter("[SubmittedAtUtc] IS NULL")
            .HasDatabaseName("IX_Reviews_OpenAssignment");
    }
}
