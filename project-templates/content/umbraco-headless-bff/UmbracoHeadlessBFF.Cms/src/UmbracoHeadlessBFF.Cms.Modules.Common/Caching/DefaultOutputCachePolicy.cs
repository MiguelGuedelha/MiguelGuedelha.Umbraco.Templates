using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Caching;

public class DefaultOutputCachePolicy : IOutputCachePolicy
{
    public const string PolicyName = "DefaultOutputCachePolicy";

    public static readonly DefaultOutputCachePolicy Instance = new();

    // private static readonly TimeSpan s_duration = TimeSpan.Parse("01:00:00");

    private DefaultOutputCachePolicy()
    {
    }

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var canCache = CanCache(context);
        context.EnableOutputCaching = canCache;
        context.AllowCacheLookup = canCache;
        context.AllowCacheStorage = canCache;
        context.AllowLocking = true;

        context.CacheVaryByRules.QueryKeys = "*";

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;

        // Verify existence of cookie headers
        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        // Check response code
        if (response.StatusCode is not StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }

    private static bool CanCache(OutputCacheContext context)
    {
        var request = context.HttpContext.Request;

        if (!HttpMethods.IsGet(request.Method) &&
            !HttpMethods.IsHead(request.Method))
        {
            return false;
        }

        return StringValues.IsNullOrEmpty(request.Headers.Authorization) &&
               request.HttpContext.User?.Identity?.IsAuthenticated != true;
    }
}
