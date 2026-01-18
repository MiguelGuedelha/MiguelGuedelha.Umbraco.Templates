using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace UmbracoHeadlessBFF.SharedModules.Common.Http;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public void AddContextItem<T>(string key, T value)
        {
            context.Items[key] = value;
        }

        public bool TryGetContextItem<T>(string key, [NotNullWhen(true)]out T? item)
        {
            var found = context.Items.TryGetValue(key, out var value);
            item = default;

            if (!found || value is not T castValue)
            {
                return false;
            }

            item = castValue;
            return true;
        }
    }
}
