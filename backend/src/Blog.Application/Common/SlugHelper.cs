using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Blog.Application.Common.Exceptions;

namespace Blog.Application.Common;

public static partial class SlugHelper
{
  public static string GenerateSlug(string value, string propertyName = "Slug")
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("Slug kaynağı boş olamaz.", nameof(value));
    }

    var folded = new StringBuilder();

    foreach (var c in value.Trim())
    {
      if (TryMapTurkish(c, out var ascii))
      {
        folded.Append(ascii);
        continue;
      }

      foreach (var d in c.ToString().Normalize(NormalizationForm.FormD))
      {
        if (CharUnicodeInfo.GetUnicodeCategory(d) == UnicodeCategory.NonSpacingMark)
        {
          continue;
        }

        folded.Append(char.ToLowerInvariant(d));
      }
    }

    var slug = folded.ToString();
    slug = NonAlphanumericRegex().Replace(slug, "-");
    slug = MultiDashRegex().Replace(slug, "-").Trim('-');

    if (string.IsNullOrEmpty(slug))
    {
      throw new AppValidationException(new Dictionary<string, string[]>
      {
        [propertyName] = ["Bu metinden URL üretilemedi."]
      });
    }

    return slug;
  }

  public static Task<string> GenerateUniqueSlugAsync(
    string value,
    string propertyName,
    Func<string, Task<bool>> slugExists,
    CancellationToken cancellationToken = default)
  {
    var baseSlug = GenerateSlug(value, propertyName);
    return EnsureUniqueSlugAsync(slugExists, baseSlug, cancellationToken);
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

  /// <summary>
  /// Invariant ToLower, İ (I+nokta) ve I/ı harflerini ASCII [a-z] dışına düşürür;
  /// regex onları siler (İnsan → nsan). URL için Türkçe harfleri burada katlıyoruz.
  /// </summary>
  private static bool TryMapTurkish(char c, out char ascii)
  {
    ascii = c switch
    {
      'ç' or 'Ç' => 'c',
      'ğ' or 'Ğ' => 'g',
      'ı' or 'I' or 'İ' => 'i',
      'ö' or 'Ö' => 'o',
      'ş' or 'Ş' => 's',
      'ü' or 'Ü' => 'u',
      _ => '\0'
    };

    return ascii != '\0';
  }

  [GeneratedRegex(@"[^a-z0-9\-]+", RegexOptions.Compiled)]
  private static partial Regex NonAlphanumericRegex();

  [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
  private static partial Regex MultiDashRegex();
}
