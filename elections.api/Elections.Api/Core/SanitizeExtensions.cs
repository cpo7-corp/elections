using Ganss.Xss;

namespace Elections.Api.Core;

public static class SanitizeExtensions
{
    private static Lazy<HtmlSanitizer> htmlSanitizer = new Lazy<HtmlSanitizer>(() => new HtmlSanitizer());

    public static string Sanitize(this string text)
    {
        htmlSanitizer.Value.AllowedTags.Clear();
        string cleanText = htmlSanitizer.Value.Sanitize(text);
        return cleanText;
    }

    public static string Clean(this string? text)
    {
        if (text == null) return string.Empty;
        return text.Trim().Sanitize();
    }

    public static bool IsEmpty(this string? s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsWEmpty(this string? s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsEmpty<T>(this List<T>? l)
    {
        return l == null || l.Count == 0;
    }

    public static List<string> CleanAndValidate(this List<string> l, uint? min = null, uint? max = null, bool isRequired = false)
    {
        if (l.IsEmpty() && isRequired == false) { return l; }

        if (l.IsEmpty() && isRequired == true)
        {
            throw new ArgumentException("List is required but is empty");
        }

        for (int i = 0; i < l.Count; i++)
        {
            l[i] = l[i]?.Clean() ?? string.Empty;

            if (isRequired == true && l[i].IsEmpty())
            {
                throw new ArgumentException($"Item {i} required but is empty");
            }

            if (min.HasValue && max.HasValue && (l[i].Length < min || l[i].Length > max))
            {
                throw new ArgumentException($"Item {i} must be between {min}-{max}");
            }

            if (min.HasValue && l[i].Length < min)
            {
                throw new ArgumentException($"Item {i} must be {min} or more");
            }

            if (max.HasValue && l[i].Length > max)
            {
                throw new ArgumentException($"Item {i} must be {max} or less");
            }
        }

        return l;
    }
}
