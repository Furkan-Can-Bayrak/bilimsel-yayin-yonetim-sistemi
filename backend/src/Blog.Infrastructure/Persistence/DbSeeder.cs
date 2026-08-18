using Blog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Blog.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await SeedAdminAsync(context, configuration, cancellationToken);
        await SeedContentAsync(context, cancellationToken);
    }

    private static async Task SeedAdminAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var username = configuration[$"{SeedOptions.SectionName}:AdminUsername"];
        var password = configuration[$"{SeedOptions.SectionName}:AdminPassword"];

        if (string.IsNullOrWhiteSpace(username))
        {
            username = "admin";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword eksik. Development: Web API → Manage User Secrets (Seed:AdminPassword). Production: Seed__AdminPassword ortam değişkeni.");
        }

        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            Username = username,
            Role = "Admin"
        };
        admin.PasswordHash = hasher.HashPassword(admin, password);

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedContentAsync(BlogDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var general = new Category
        {
            Name = "Genel",
            Slug = "genel"
        };

        context.Categories.Add(general);
        await context.SaveChangesAsync(cancellationToken);

        context.Posts.AddRange(
            new Post
            {
                Title = "Merhaba Blog",
                Slug = "merhaba-blog",
                Summary = "İlk örnek yazı — onion + EF Core öğrenme projesi.",
                Content = "Bu yazı Faz 2 seed verisidir. CQRS ve Angular sonraki fazlarda gelecek.",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow,
                CategoryId = general.Id
            },
            new Post
            {
                Title = "Onion Mimari Notları",
                Slug = "onion-mimari-notlari",
                Summary = "Domain, Application, Infrastructure ve API katmanları.",
                Content = "Domain çekirdektir; Infrastructure EF Core ile veritabanına bağlanır.",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow,
                CategoryId = general.Id
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
