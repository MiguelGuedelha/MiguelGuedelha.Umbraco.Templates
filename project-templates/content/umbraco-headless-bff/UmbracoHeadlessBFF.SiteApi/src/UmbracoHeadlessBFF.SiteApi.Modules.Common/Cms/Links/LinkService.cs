using Microsoft.Extensions.Options;
using UmbracoHeadlessBFF.SharedModules.Cms.Links;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;
using ZiggyCreatures.Caching.Fusion;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.Links;

public sealed class LinkService
{
    private readonly ILinksApi _linksApi;
    private readonly SiteResolutionContext _siteResolutionContext;
    private readonly IFusionCache _fusionCache;
    private readonly SiteApiCachingOptions _siteApiCachingOptions;

    public LinkService(ILinksApi linksApi,
        SiteResolutionContext siteResolutionContext,
        IFusionCacheProvider fusionCacheProvider,
        IOptionsSnapshot<SiteApiCachingOptions> siteApiCachingOptions)
    {
        _linksApi = linksApi;
        _siteResolutionContext = siteResolutionContext;
        _fusionCache = fusionCacheProvider.GetCache(CachingConstants.SiteApi.CacheName);
        _siteApiCachingOptions = siteApiCachingOptions.Value;
    }

    public async Task<Link?> ResolveLink(Guid id, string? culture)
    {
        var linkCulture = culture ?? _siteResolutionContext.Site.CultureInfo;
        var domain = _siteResolutionContext.Domain;

        if (_siteResolutionContext.IsPreview)
        {
            var response = await _linksApi.GetLink(id, linkCulture, domain, true);
            return response.Content;
        }

        var data = await _fusionCache.GetOrSetAsync<Link?>(
            CacheKeyExtensions.GetLinkKey(id, linkCulture, domain),
            async (ctx, ct) =>
            {
                var response = await _linksApi.GetLink(id, linkCulture, domain, false, ct);

                if (response is { IsSuccessful: true, Content: not null })
                {
                    return response.Content;
                }

                ctx.Options.SetAllDurations(_siteApiCachingOptions.Default.NullDuration);

                return null;
            },
            tags: [CachingConstants.SiteApi.Tags.Links, id.ToString(), linkCulture]);

        return data;
    }
}
