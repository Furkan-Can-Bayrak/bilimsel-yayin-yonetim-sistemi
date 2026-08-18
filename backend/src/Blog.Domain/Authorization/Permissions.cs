using System.Reflection;

namespace Blog.Domain.Authorization;

/// <summary>
/// Sistemdeki tüm izinler. İzinler kodda sabittir çünkü her biri kodun bir yerinde
/// kontrol edilir. Roller ise veritabanı kaydıdır; yeni rol eklemek kod değişikliği
/// gerektirmez, yeni izin eklemek gerektirir.
/// </summary>
public static class Permissions
{
    public static class Manuscripts
    {
        public const string Create = "Manuscript.Create";
        public const string Update = "Manuscript.Update";
        public const string Delete = "Manuscript.Delete";
        public const string Submit = "Manuscript.Submit";
        public const string Decide = "Manuscript.Decide";
        public const string Publish = "Manuscript.Publish";
        public const string Unpublish = "Manuscript.Unpublish";

        /// <summary>Başkalarının taslakları da dahil tüm makaleleri görme.</summary>
        public const string ViewAll = "Manuscript.ViewAll";
    }

    public static class Reviews
    {
        public const string Assign = "Review.Assign";
        public const string Submit = "Review.Submit";
        public const string ViewAll = "Review.ViewAll";
    }

    public static class ResearchAreas
    {
        public const string Manage = "ResearchArea.Manage";
    }

    public static class Users
    {
        public const string View = "User.View";
        public const string Manage = "User.Manage";
    }

    public static class Roles
    {
        public const string View = "Role.View";
        public const string Manage = "Role.Manage";
    }

    public static class Notifications
    {
        public const string View = "Notification.View";
    }

    /// <summary>
    /// Yukarıdaki tüm sabitler. Seed sırasında veritabanına yazılacak liste buradan gelir,
    /// böylece yeni bir sabit eklemek onu otomatik olarak seed kapsamına alır.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = typeof(Permissions)
        .GetNestedTypes(BindingFlags.Public)
        .SelectMany(group => group.GetFields(BindingFlags.Public | BindingFlags.Static))
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .OrderBy(code => code)
        .ToArray();
}
