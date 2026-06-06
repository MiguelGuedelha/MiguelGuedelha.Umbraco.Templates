using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching.Policies;

public sealed class SiteBasedOutputCachePolicy : SiteApiOutputCachePolicyBase, IOutputCachePolicy
{
    public const string PolicyName = "SiteBasedOutputCachePolicy";

    public static readonly SiteBasedOutputCachePolicy Instance = new();

    private SiteBasedOutputCachePolicy()
    {
    }

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        context.EnableOutputCaching = true;
        context.AllowLocking = true;

        var siteResolutionContext = context.HttpContext.RequestServices.GetService<SiteResolutionContext>();

        if (siteResolutionContext is null)
        {
            context.EnableOutputCaching = false;
            context.AllowCacheLookup = false;
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        var canCache = CanCacheBase(context, siteResolutionContext, out var siteId);
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = canCache;
        context.AllowCacheStorage = canCache;

        if (canCache)
        {
            context.CacheVaryByRules.VaryByValues["siteId"] = siteId!;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        ServeResponseBaseAsync(context);
        try
        {
            var siteResolutionContext = context.HttpContext.RequestServices.GetRequiredService<SiteResolutionContext>();

            var tags = new List<string?>
            {
                siteResolutionContext.Site.DictionaryId.ToString(),
                siteResolutionContext.Site.NotFoundPageId?.ToString(),
                siteResolutionContext.Site.SiteSettingsId.ToString()
            };

            foreach (var tag in tags.Where(x => x is not null))
            {
                context.Tags.Add(tag!);
            }
        }
        catch
        {
            context.AllowCacheStorage = false;
            throw;
        }
        return ValueTask.CompletedTask;
    }
}
