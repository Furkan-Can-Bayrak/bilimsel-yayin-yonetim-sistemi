using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Blog.Application.Common;

public static partial class UserEmailHelper
{
    public static string BuildLocalPart(string firstName, string lastName)
    {
        var initials = SplitNameParts(firstName)
            .Select(part => FoldToAscii(part)[0])
            .ToArray();

        var surname = FoldToAscii(lastName.Trim());
        if (string.IsNullOrEmpty(surname) || initials.Length == 0)
        {
            throw new ArgumentException("Ad ve soyaddan e-posta üretilemedi.");
        }

        return new string(initials) + surname;
    }

    public static string BuildEmail(string localPart, string domain)
    {
        var local = localPart.Trim().ToLowerInvariant();
        var host = NormalizeDomain(domain);
        return $"{local}@{host}";
    }

    public static async Task<string> BuildUniqueEmailAsync(
        string firstName,
        string lastName,
        string domain,
        Func<string, Task<bool>> emailExists,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var local = BuildLocalPart(firstName, lastName);
        var host = NormalizeDomain(domain);
        var email = $"{local}@{host}";
        var suffix = 2;

        while (await emailExists(email))
        {
            cancellationToken.ThrowIfCancellationRequested();
            email = $"{local}{suffix}@{host}";
            suffix++;
        }

        return email;
    }

    public static string GeneratePassword(int length = 12)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }

    public static string NormalizeDomain(string domain)
    {
        var host = domain.Trim().TrimStart('@').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("Kurum e-posta alanı boş olamaz.", nameof(domain));
        }

        return host;
    }

    private static IEnumerable<string> SplitNameParts(string firstName)
    {
        return firstName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => FoldToAscii(part).Length > 0);
    }

    private static string FoldToAscii(string value)
    {
        var folded = new StringBuilder();

        foreach (var c in value)
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

                if (char.IsLetterOrDigit(d))
                {
                    folded.Append(char.ToLowerInvariant(d));
                }
            }
        }

        return NonLetterDigitRegex().Replace(folded.ToString(), string.Empty);
    }

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

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonLetterDigitRegex();
}
