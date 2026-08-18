using Blog.Domain.Authorization;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Blog.Infrastructure.Persistence;

public static class DbSeeder
{
    private const string AdminRoleName = "Admin";
    private const string EditorRoleName = "Editor";
    private const string ReviewerRoleName = "Reviewer";
    private const string AuthorRoleName = "Author";

    public static async Task SeedAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await SeedPermissionsAsync(context, cancellationToken);
        await SeedRolesAsync(context, cancellationToken);
        await SyncSystemRolePermissionsAsync(context, cancellationToken);
        await SeedUsersAsync(context, configuration, cancellationToken);
        await SyncDevelopmentPasswordsAsync(context, configuration, cancellationToken);
        await SeedContentAsync(context, cancellationToken);
    }

    /// <summary>
    /// Koddaki izin sabitlerini tabloya taşır. Yalnızca eksikleri ekler; mevcut kayıtlara
    /// dokunmaz ve koddan kaldırılmış izinleri silmez (rollere bağlı olabilirler).
    /// </summary>
    private static async Task SeedPermissionsAsync(
        BlogDbContext context,
        CancellationToken cancellationToken)
    {
        var existingCodes = await context.Permissions
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var missingCodes = Permissions.All.Except(existingCodes).ToList();

        if (missingCodes.Count == 0)
        {
            return;
        }

        context.Permissions.AddRange(missingCodes.Select(code => new Permission
        {
            Code = code,
            Description = PermissionDescriptions.GetValueOrDefault(code)
        }));

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(
        BlogDbContext context,
        CancellationToken cancellationToken)
    {
        var permissionIdsByCode = await context.Permissions
            .ToDictionaryAsync(p => p.Code, p => p.Id, cancellationToken);

        var existingRoleNames = await context.Roles
            .IgnoreQueryFilters()
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        foreach (var (roleName, definition) in RoleDefinitions)
        {
            if (existingRoleNames.Contains(roleName))
            {
                continue;
            }

            var role = new Role
            {
                Name = roleName,
                Description = definition.Description,
                IsSystemRole = definition.IsSystemRole
            };

            foreach (var code in definition.PermissionCodes)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    PermissionId = permissionIdsByCode[code]
                });
            }

            context.Roles.Add(role);
        }

        await context.SaveChangesAsync(cancellationToken);
        await GrantAllPermissionsToAdminAsync(context, cancellationToken);
    }

    /// <summary>
    /// Sistem rollerine kodda tanımlı eksik izinleri ekler; panelden verilen fazlalara dokunmaz.
    /// SeedRolesAsync var olan rolleri atladığı için yeni bir sabit (ör. Decide) aksi halde
    /// yalnızca Admin'e giderdi.
    /// </summary>
    private static async Task SyncSystemRolePermissionsAsync(
        BlogDbContext context,
        CancellationToken cancellationToken)
    {
        var permissionIdsByCode = await context.Permissions
            .ToDictionaryAsync(p => p.Code, p => p.Id, cancellationToken);

        var roles = await context.Roles
            .IgnoreQueryFilters()
            .Include(r => r.RolePermissions)
            .Where(r => r.IsSystemRole)
            .ToListAsync(cancellationToken);

        foreach (var role in roles)
        {
            if (!RoleDefinitions.TryGetValue(role.Name, out var definition))
            {
                continue;
            }

            var grantedIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            foreach (var code in definition.PermissionCodes)
            {
                if (!permissionIdsByCode.TryGetValue(code, out var permissionId) ||
                    grantedIds.Contains(permissionId))
                {
                    continue;
                }

                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
                grantedIds.Add(permissionId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await GrantAllPermissionsToAdminAsync(context, cancellationToken);
    }

    /// <summary>
    /// Admin rolünün her zaman tüm izinlere sahip olmasını garanti eder. Kodda yeni bir izin
    /// tanımlandığında Admin onu otomatik kazanır; panelden çıkarılsa bile yeniden eklenir.
    /// </summary>
    private static async Task GrantAllPermissionsToAdminAsync(
        BlogDbContext context,
        CancellationToken cancellationToken)
    {
        var adminRole = await context.Roles
            .IgnoreQueryFilters()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == AdminRoleName, cancellationToken);

        if (adminRole is null)
        {
            return;
        }

        var grantedPermissionIds = adminRole.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var missingPermissionIds = await context.Permissions
            .Where(p => !grantedPermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (missingPermissionIds.Count == 0)
        {
            return;
        }

        foreach (var permissionId in missingPermissionIds)
        {
            adminRole.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permissionId
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUsersAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        // Silinmiş kullanıcılar da sayılmalı: e-posta index'i onları da kapsıyor,
        // aksi halde tekrar eklemeye çalışıp unique ihlaline düşerdik.
        if (await context.Users.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var adminEmail = configuration[$"{SeedOptions.SectionName}:AdminEmail"];
        var adminPassword = configuration[$"{SeedOptions.SectionName}:AdminPassword"];
        var demoPassword = configuration[$"{SeedOptions.SectionName}:DemoPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = "admin@yayin.local";
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword eksik. Development: Blog.API → Manage User Secrets (Seed:AdminPassword). Production: Seed__AdminPassword ortam değişkeni.");
        }

        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            demoPassword = adminPassword;
        }

        var roleIdsByName = await context.Roles
            .IgnoreQueryFilters()
            .ToDictionaryAsync(r => r.Name, r => r.Id, cancellationToken);

        var hasher = new PasswordHasher<User>();
        var createdAtUtc = DateTime.UtcNow;

        var seedUsers = new[]
        {
            (
                Email: adminEmail,
                Password: adminPassword,
                FirstName: "Sistem",
                LastName: "Yöneticisi",
                Title: (string?)null,
                Affiliation: (string?)null,
                Orcid: (string?)null,
                RoleNames: new[] { AdminRoleName }
            ),
            (
                Email: "editor@yayin.local",
                Password: demoPassword,
                FirstName: "Selin",
                LastName: "Aydın",
                Title: (string?)"Prof. Dr.",
                Affiliation: (string?)"İstanbul Teknik Üniversitesi",
                Orcid: (string?)"0000-0001-2345-6789",
                RoleNames: new[] { EditorRoleName }
            ),
            (
                // Aynı kişinin hem hakem hem yazar olması gerçek hayatta olağan;
                // çoklu rol yapısını gösteren örnek bu.
                Email: "reviewer@yayin.local",
                Password: demoPassword,
                FirstName: "Mert",
                LastName: "Kaya",
                Title: (string?)"Doç. Dr.",
                Affiliation: (string?)"Ege Üniversitesi",
                Orcid: (string?)"0000-0002-3456-7890",
                RoleNames: new[] { ReviewerRoleName, AuthorRoleName }
            ),
            (
                Email: "author@yayin.local",
                Password: demoPassword,
                FirstName: "Elif",
                LastName: "Demir",
                Title: (string?)"Dr. Öğr. Üyesi",
                Affiliation: (string?)"Orta Doğu Teknik Üniversitesi",
                Orcid: (string?)"0000-0003-4567-8901",
                RoleNames: new[] { AuthorRoleName }
            )
        };

        foreach (var seedUser in seedUsers)
        {
            var user = new User
            {
                // Login gelen adresi küçük harfe çevirdiği için burada da normalize ediyoruz.
                Email = seedUser.Email.Trim().ToLowerInvariant(),
                AcademicTitle = seedUser.Title,
                Affiliation = seedUser.Affiliation,
                Orcid = seedUser.Orcid,
                IsActive = true,
                CreatedAtUtc = createdAtUtc
            };

            user.SetName(seedUser.FirstName, seedUser.LastName);

            user.PasswordHash = hasher.HashPassword(user, seedUser.Password);

            foreach (var roleName in seedUser.RoleNames)
            {
                user.UserRoles.Add(new UserRole { RoleId = roleIdsByName[roleName] });
            }

            context.Users.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Kullanıcılar bir kez oluşturulunca seed tekrar çalışmaz; User Secrets'taki
    /// DemoPassword değişse bile hash eski kalır. Development'ta DemoPassword verildiyse
    /// dört hesap da o şifreye çekilir (yerel girişin "hatalı" kalmaması için).
    /// </summary>
    private static async Task SyncDevelopmentPasswordsAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var demoPassword = configuration[$"{SeedOptions.SectionName}:DemoPassword"];
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return;
        }

        var adminEmail = configuration[$"{SeedOptions.SectionName}:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = "admin@yayin.local";
        }

        adminEmail = adminEmail.Trim().ToLowerInvariant();

        var emails = new[]
        {
            adminEmail,
            "editor@yayin.local",
            "reviewer@yayin.local",
            "author@yayin.local"
        };

        var users = await context.Users
            .IgnoreQueryFilters()
            .Where(u => emails.Contains(u.Email))
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return;
        }

        var hasher = new PasswordHasher<User>();
        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, demoPassword);
            user.SecurityVersion += 1;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedContentAsync(BlogDbContext context, CancellationToken cancellationToken)
    {
        if (await context.ResearchAreas.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var computerScience = new ResearchArea
        {
            Name = "Bilgisayar Bilimleri",
            Slug = "bilgisayar-bilimleri"
        };

        context.ResearchAreas.Add(computerScience);
        await context.SaveChangesAsync(cancellationToken);

        var author = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "author@yayin.local", cancellationToken)
            ?? await context.Users.IgnoreQueryFilters().FirstAsync(cancellationToken);

        context.Manuscripts.AddRange(
            new Manuscript
            {
                Title = "Derin Öğrenme ile Makale Sınıflandırma",
                Slug = "derin-ogrenme-ile-makale-siniflandirma",
                Summary = "Bilimsel metinlerin araştırma alanına otomatik atanması üzerine bir çalışma.",
                Content = "Bu örnek makale seed verisidir. Değerlendirme ve yayın akışı sonraki adımlarda genişleyecek.",
                Status = ManuscriptStatus.Published,
                PublishedAt = DateTime.UtcNow,
                ResearchAreaId = computerScience.Id,
                AuthorId = author.Id
            },
            new Manuscript
            {
                Title = "Açık Erişim Dergilerinde Hakem Atama",
                Slug = "acik-erisim-dergilerinde-hakem-atama",
                Summary = "Hakem yükünün araştırma alanına göre dengelenmesi.",
                Content = "Bu örnek makale, editör kararlarının ve hakem atamasının sistemde nasıl modelleneceğini gösterir.",
                Status = ManuscriptStatus.Published,
                PublishedAt = DateTime.UtcNow,
                ResearchAreaId = computerScience.Id,
                AuthorId = author.Id
            });

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record RoleDefinition(
        string Description,
        bool IsSystemRole,
        string[] PermissionCodes);

    private static readonly Dictionary<string, RoleDefinition> RoleDefinitions = new()
    {
        [AdminRoleName] = new RoleDefinition(
            "Sistem yöneticisi. Tüm izinlere sahiptir.",
            IsSystemRole: true,
            PermissionCodes: []),

        [EditorRoleName] = new RoleDefinition(
            "Editör. Hakem atar, nihai kararı verir ve yayınlar.",
            IsSystemRole: true,
            PermissionCodes:
            [
                Permissions.Manuscripts.ViewAll,
                Permissions.Manuscripts.Decide,
                Permissions.Manuscripts.Publish,
                Permissions.Manuscripts.Unpublish,
                Permissions.Reviews.Assign,
                Permissions.Reviews.ViewAll,
                Permissions.ResearchAreas.Manage,
                Permissions.Notifications.View
            ]),

        [ReviewerRoleName] = new RoleDefinition(
            "Hakem. Kendisine atanan makaleleri değerlendirir.",
            IsSystemRole: true,
            PermissionCodes:
            [
                Permissions.Reviews.Submit,
                Permissions.Notifications.View
            ]),

        [AuthorRoleName] = new RoleDefinition(
            "Yazar. Makale hazırlar ve değerlendirmeye gönderir.",
            IsSystemRole: true,
            PermissionCodes:
            [
                Permissions.Manuscripts.Create,
                Permissions.Manuscripts.Update,
                Permissions.Manuscripts.Submit,
                Permissions.Notifications.View
            ])
    };

    private static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        [Permissions.Manuscripts.Create] = "Makale taslağı oluşturma",
        [Permissions.Manuscripts.Update] = "Makale düzenleme",
        [Permissions.Manuscripts.Delete] = "Makale silme",
        [Permissions.Manuscripts.Submit] = "Makaleyi değerlendirmeye gönderme",
        [Permissions.Manuscripts.Decide] = "Makaleyi kabul veya ret etme",
        [Permissions.Manuscripts.Publish] = "Makaleyi yayınlama",
        [Permissions.Manuscripts.Unpublish] = "Yayını geri alma",
        [Permissions.Manuscripts.ViewAll] = "Taslaklar dahil tüm makaleleri görme",
        [Permissions.Reviews.Assign] = "Hakem atama",
        [Permissions.Reviews.Submit] = "Değerlendirme girme",
        [Permissions.Reviews.ViewAll] = "Tüm değerlendirmeleri görme",
        [Permissions.ResearchAreas.Manage] = "Araştırma alanlarını yönetme",
        [Permissions.Users.View] = "Kullanıcıları görme",
        [Permissions.Users.Manage] = "Kullanıcıları yönetme",
        [Permissions.Roles.View] = "Rolleri görme",
        [Permissions.Roles.Manage] = "Rolleri ve izinlerini yönetme",
        [Permissions.Notifications.View] = "Bildirimleri görme"
    };
}
