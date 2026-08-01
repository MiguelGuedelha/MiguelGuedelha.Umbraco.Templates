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

        public string SanitisePathSlashes()
        {
            return value.Equals("/") ? value : $"/{value.Trim(s_uriTrimChars)}/";
        }
    }
}
