using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("Institutions");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Abbreviation)
            .HasMaxLength(20);

        builder.HasIndex(i => i.Name)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasQueryFilter(i => i.DeletedAtUtc == null);
    }
}
