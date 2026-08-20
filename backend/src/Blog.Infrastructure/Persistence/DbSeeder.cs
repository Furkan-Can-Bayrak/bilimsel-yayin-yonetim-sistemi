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
    private const string EditorRoleName = "Editör";
    private const string ReviewerRoleName = "Hakem";
    private const string AuthorRoleName = "Yazar";

    private const string DefaultAdminEmail = "fcbayrak@firat.edu.tr";
    private const string AdminInstitutionName = "Fırat Üniversitesi";

    public static async Task SeedAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await SeedPermissionsAsync(context, cancellationToken);
        await SeedRolesAsync(context, cancellationToken);
        await SyncSystemRolePermissionsAsync(context, cancellationToken);
        await SeedInstitutionsAsync(context, cancellationToken);
        await EnsureAdminUserAsync(context, configuration, cancellationToken);
        await SyncDevelopmentPasswordsAsync(context, configuration, cancellationToken);
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
    /// eklendiğinde panelden elle vermeye gerek kalmaz.
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

        var allPermissionIds = await context.Permissions
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in allPermissionIds)
        {
            if (grantedPermissionIds.Contains(permissionId))
            {
                continue;
            }

            adminRole.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permissionId
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static readonly (string Name, string? Abbreviation, string EmailDomain)[] SeedInstitutions =
    [
        ("Orta Doğu Teknik Üniversitesi", "ODTÜ", "metu.edu.tr"),
        ("İstanbul Teknik Üniversitesi", "İTÜ", "itu.edu.tr"),
        ("Ege Üniversitesi", null, "ege.edu.tr"),
        ("Ankara Üniversitesi", null, "ankara.edu.tr"),
        ("Hacettepe Üniversitesi", null, "hacettepe.edu.tr"),
        ("Boğaziçi Üniversitesi", null, "boun.edu.tr"),
        ("İstanbul Üniversitesi", null, "istanbul.edu.tr"),
        ("Gazi Üniversitesi", null, "gazi.edu.tr"),
        ("Yıldız Teknik Üniversitesi", "YTÜ", "yildiz.edu.tr"),
        ("Marmara Üniversitesi", null, "marmara.edu.tr"),
        ("Dokuz Eylül Üniversitesi", null, "deu.edu.tr"),
        ("Çukurova Üniversitesi", null, "cu.edu.tr"),
        ("Karadeniz Teknik Üniversitesi", "KTÜ", "ktu.edu.tr"),
        ("Atatürk Üniversitesi", null, "atauni.edu.tr"),
        ("Erciyes Üniversitesi", null, "erciyes.edu.tr"),
        ("Selçuk Üniversitesi", null, "selcuk.edu.tr"),
        ("Bursa Uludağ Üniversitesi", null, "uludag.edu.tr"),
        ("Akdeniz Üniversitesi", null, "akdeniz.edu.tr"),
        ("Gaziantep Üniversitesi", null, "gantep.edu.tr"),
        ("Fırat Üniversitesi", null, "firat.edu.tr"),
        ("Sabancı Üniversitesi", null, "sabanciuniv.edu"),
        ("Koç Üniversitesi", null, "ku.edu.tr"),
        ("Bilkent Üniversitesi", null, "bilkent.edu.tr")
    ];

    /// <summary>
    /// Eksik kurumları ekler; mevcut kayıtlarda boş EmailDomain varsa doldurur.
    /// </summary>
    private static async Task SeedInstitutionsAsync(
        BlogDbContext context,
        CancellationToken cancellationToken)
    {
        var institutions = await context.Institutions
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var byName = institutions.ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
        var changed = false;

        foreach (var seed in SeedInstitutions)
        {
            if (byName.TryGetValue(seed.Name, out var existing))
            {
                if (string.IsNullOrWhiteSpace(existing.EmailDomain))
                {
                    existing.EmailDomain = seed.EmailDomain;
                    changed = true;
                }

                continue;
            }

            context.Institutions.Add(new Institution
            {
                Name = seed.Name,
                Abbreviation = seed.Abbreviation,
                EmailDomain = seed.EmailDomain
            });
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string ResolveAdminEmail(IConfiguration configuration)
    {
        var adminEmail = configuration[$"{SeedOptions.SectionName}:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = DefaultAdminEmail;
        }

        return adminEmail.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Tek yönetici hesabını oluşturur veya profilini günceller.
    /// </summary>
    private static async Task EnsureAdminUserAsync(
        BlogDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var adminEmail = ResolveAdminEmail(configuration);
        var adminPassword = configuration[$"{SeedOptions.SectionName}:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword eksik. Development: Blog.API → Manage User Secrets (Seed:AdminPassword). Production: Seed__AdminPassword ortam değişkeni.");
        }

        var adminRoleId = await context.Roles
            .IgnoreQueryFilters()
            .Where(r => r.Name == AdminRoleName)
            .Select(r => r.Id)
            .FirstAsync(cancellationToken);

        var institutionId = await context.Institutions
            .IgnoreQueryFilters()
            .Where(i => i.Name == AdminInstitutionName)
            .Select(i => (int?)i.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (institutionId is null)
        {
            throw new InvalidOperationException(
                $"Seed kurumu bulunamadı: {AdminInstitutionName}");
        }

        var hasher = new PasswordHasher<User>();
        var user = await context.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = adminEmail,
                AcademicTitle = AcademicTitle.DocDr,
                InstitutionId = institutionId,
                Orcid = "4382-9384-2256-2216",
                IsActive = true,
                SecurityVersion = 1,
                CreatedAtUtc = DateTime.UtcNow
            };

            user.SetName("Furkan Can", "BAYRAK");
            user.PasswordHash = hasher.HashPassword(user, adminPassword);
            user.UserRoles.Add(new UserRole { RoleId = adminRoleId });
            context.Users.Add(user);
        }
        else
        {
            user.DeletedAtUtc = null;
            user.SetName("Furkan Can", "BAYRAK");
            user.AcademicTitle = AcademicTitle.DocDr;
            user.InstitutionId = institutionId;
            user.Orcid = "4382-9384-2256-2216";
            user.IsActive = true;

            if (!user.UserRoles.Any(ur => ur.RoleId == adminRoleId))
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRoleId });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Development'ta DemoPassword verildiyse yönetici hesabının şifresini buna eşitler.
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

        var adminEmail = ResolveAdminEmail(configuration);

        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);

        if (user is null)
        {
            return;
        }

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, demoPassword);
        user.SecurityVersion += 1;

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
