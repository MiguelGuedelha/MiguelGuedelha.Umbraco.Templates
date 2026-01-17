using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using UmbracoHeadlessBFF.SharedModules.Cms.Sitemap;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching.Policies;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Endpoints;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Errors;
using ZiggyCreatures.Caching.Fusion;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Endpoints;

internal static class GetSitemapEndpoint
{
    private const string SitemapSegment = "sitemap.xml";

    extension(RouteGroupBuilder builder)
    {
        public RouteGroupBuilder MapGetSitemap()
        {
            builder
                .MapGet("/sitemap", Handle)
                .MapToApiVersion(EndpointConstants.Versions.V1)
                .CacheOutput(SiteAndPathBasedOutputCachePolicy.PolicyName);

            return builder;
        }
    }

    private static async Task<Results<Ok<SitemapData>, NotFound>> Handle(
        ISitemapsApi sitemapsApi,
        SiteResolutionContext siteResolutionContext,
        IFusionCacheProvider fusionCacheProvider,
        IOptionsSnapshot<SiteApiCachingOptions> siteApiCachingOptions)
    {
        var path = siteResolutionContext.Path;
        var domain = siteResolutionContext.Domain;

        var domainEntry = siteResolutionContext.Site.Domains
            .First(x => x.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));

        var sitemapPath = path.Replace(domainEntry.Path, string.Empty);

        if (!sitemapPath.Equals(SitemapSegment, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.NotFound();
        }

        var homepageId = siteResolutionContext.Site.HomepageId;
        var culture = siteResolutionContext.Site.CultureInfo;
        var siteSettingsId = siteResolutionContext.Site.SiteSettingsId;

        if (siteResolutionContext.IsPreview)
        {
            var response = await GetSitemapFactory(homepageId, culture, true);

            return response switch
            {
                { Content: { Items.Count: > 0 } content } => TypedResults.Ok(content),
                _ => TypedResults.NotFound(),
            };
        }

        var fusionCache = fusionCacheProvider.GetCache(CachingConstants.SiteApi.CacheName);

        var data = await fusionCache.GetOrSetAsync<SitemapData?>(
            CacheKeyExtensions.GetSitemapKey(homepageId, culture),
            async (ctx, ct) =>
            {
                var response = await GetSitemapFactory(homepageId, culture, true, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(siteApiCachingOptions.Value.Default.NullDuration));
                }

                return response.Content;
            },
            tags: [CachingConstants.SiteApi.Tags.Sitemaps, homepageId.ToString(), culture, siteSettingsId.ToString()]);

        return data switch
        {
            { Items.Count: > 0 } => TypedResults.Ok(data),
            _ => TypedResults.NotFound(),
        };

        async Task<IApiResponse<SitemapData>> GetSitemapFactory(Guid factoryHome, string factoryCulture, bool factoryPreview, CancellationToken cancellationToken = default)
        {
            var response = await sitemapsApi.GetSitemap(factoryHome, factoryCulture, factoryPreview, cancellationToken);

            return response is { IsSuccessful: false, StatusCode: not HttpStatusCode.NotFound }
                ? throw new SiteApiException((int)response.StatusCode, response.ReasonPhrase)
                : response;
        }
    }
}
