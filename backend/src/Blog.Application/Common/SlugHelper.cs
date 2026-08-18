using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Blog.Application.Common;

public static partial class SlugHelper
{
  public static string GenerateFromTitle(string title)
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return "post";
    }

    var normalized = title.Trim().ToLowerInvariant();
    normalized = normalized.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();

    foreach (var c in normalized)
    {
      if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
      {
        sb.Append(c);
      }
    }

    var slug = sb.ToString().Normalize(NormalizationForm.FormC);
    slug = NonAlphanumericRegex().Replace(slug, "-");
    slug = MultiDashRegex().Replace(slug, "-").Trim('-');

    return string.IsNullOrEmpty(slug) ? "post" : slug;
  }

  public static async Task<string> EnsureUniqueSlugAsync(
    Func<string, Task<bool>> slugExists,
    string baseSlug,
    CancellationToken cancellationToken = default)
  {
    var slug = baseSlug;
    var suffix = 2;

    while (await slugExists(slug))
    {
      slug = $"{baseSlug}-{suffix}";
      suffix++;
    }

    return slug;
  }

  [GeneratedRegex(@"[^a-z0-9\-]+", RegexOptions.Compiled)]
  private static partial Regex NonAlphanumericRegex();

  [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
  private static partial Regex MultiDashRegex();
}
