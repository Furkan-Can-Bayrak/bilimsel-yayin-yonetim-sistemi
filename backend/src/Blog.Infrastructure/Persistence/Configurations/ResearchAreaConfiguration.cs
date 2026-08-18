using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations;

public class ResearchAreaConfiguration : IEntityTypeConfiguration<ResearchArea>
{
    public void Configure(EntityTypeBuilder<ResearchArea> builder)
    {
        builder.ToTable("ResearchAreas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasQueryFilter(a => a.DeletedAtUtc == null);
    }
}
