using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations;

public class ManuscriptConfiguration : IEntityTypeConfiguration<Manuscript>
{
    public void Configure(EntityTypeBuilder<Manuscript> builder)
    {
        builder.ToTable("Manuscripts");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Slug)
            .IsRequired()
            .HasMaxLength(220);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.Summary)
            .HasMaxLength(500);

        builder.Property(m => m.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(m => m.Slug)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasIndex(m => m.AuthorId);
        builder.HasIndex(m => m.Status);

        builder.HasOne(m => m.ResearchArea)
            .WithMany(a => a.Manuscripts)
            .HasForeignKey(m => m.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(m => m.DeletedAtUtc == null);
    }
}
