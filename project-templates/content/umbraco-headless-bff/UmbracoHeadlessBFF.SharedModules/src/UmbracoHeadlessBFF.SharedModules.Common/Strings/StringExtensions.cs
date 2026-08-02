using System.Net;

namespace UmbracoHeadlessBFF.SharedModules.Common.Strings;

public static class StringExtensions
{
    private static readonly char[] s_uriTrimChars = ['/', '\\'];

    extension(string value)
    {
        public string CombineUri(params string[] parts)
        {
            return parts is { Length: 0 }
                ? value
                : parts.Aggregate(value, (current, t) => $"{current.TrimEnd(s_uriTrimChars)}/{t.TrimStart(s_uriTrimChars)}");
        }

        public string SanitisePath()
        {
            if (value.Contains("%2F", StringComparison.OrdinalIgnoreCase))
            {
                value = WebUtility.UrlDecode(value);
            }

            return value.Equals("/") ? value : $"/{value.Trim(s_uriTrimChars)}/";
        }

        public string EnsureHttpScheme(bool forceHttps = false)
        {
            if (!forceHttps)
            {
                return value.StartsWith("http") ? value : $"https://{value}";
            }

            return value.StartsWith("http") ? value.Replace("http:", "https:") : $"https://{value}";
        }
    }
}
