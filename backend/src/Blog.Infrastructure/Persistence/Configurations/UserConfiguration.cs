using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(u => u.AcademicTitle)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(AcademicTitle.Dr)
            .HasSentinel(default(AcademicTitle));

        builder.HasOne(u => u.Institution)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.InstitutionId);

        // ORCID her zaman 19 karakter: 0000-0002-1825-0097
        builder.Property(u => u.Orcid)
            .HasMaxLength(19)
            .IsFixedLength();

        // Silinmiş kayıtlar dahil global tekil. E-posta kişinin kimliği olduğu için
        // rezerve kalır; dönen kullanıcıya yeni hesap değil, eski kaydı geri verilir.
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // ORCID küresel araştırmacı kimliği; o da silinmiş kayıtlar dahil tekil.
        // Filtre yalnızca NULL'lar için: SQL Server nullable kolonda düz unique index
        // kullanıldığında ikinci NULL'ı yinelenen sayıp reddeder.
        builder.HasIndex(u => u.Orcid)
            .IsUnique()
            .HasFilter("[Orcid] IS NOT NULL");

        builder.Ignore(u => u.DisplayName);
        builder.Ignore(u => u.DisplayNameWithTitle);
    }
}
