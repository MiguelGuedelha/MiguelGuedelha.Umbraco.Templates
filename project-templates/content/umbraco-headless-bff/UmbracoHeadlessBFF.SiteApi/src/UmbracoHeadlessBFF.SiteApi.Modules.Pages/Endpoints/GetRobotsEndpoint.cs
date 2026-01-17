using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;
using UmbracoHeadlessBFF.SharedModules.Cms.Robots;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching.Policies;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Endpoints;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Errors;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.Pages;
using ZiggyCreatures.Caching.Fusion;
using NotFound = Microsoft.AspNetCore.Http.HttpResults.NotFound;



namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Endpoints;

internal static class GetRobotsEndpoint
{
    private const string RobotsTxtSegment = "robots.txt";

    extension(RouteGroupBuilder builder)
    {
        public RouteGroupBuilder MapGetRobots()
        {
            builder
                .MapGet("/robots", Handle)
                .MapToApiVersion(EndpointConstants.Versions.V1)
                .CacheOutput(SiteAndPathBasedOutputCachePolicy.PolicyName);

            return builder;
        }
    }

    private static async Task<Results<Ok<RobotsTxt>, NotFound>> Handle(
        IRobotsApi robotsApi,
        SiteResolutionContext siteResolutionContext,
        IFusionCacheProvider fusionCacheProvider,
        IOptionsSnapshot<SiteApiCachingOptions> siteApiCachingOptions,
        IPagesService pagesService,
        SiteResolutionService siteResolutionService)
    {
        var path = siteResolutionContext.Path;
        var domain = siteResolutionContext.Domain;

        var domainEntry = siteResolutionContext.Site.Domains
            .First(x => x.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));

        var robotsPath = path.Replace(domainEntry.Path, string.Empty);

        if (!robotsPath.Equals(RobotsTxtSegment, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.NotFound();
        }

        var homepageId = siteResolutionContext.Site.HomepageId;
        var culture = siteResolutionContext.Site.CultureInfo;
        var isPreview = siteResolutionContext.IsPreview;
        var siteSettingsId = siteResolutionContext.Site.SiteSettingsId;

        string? content;

        if (isPreview)
        {
            var response = await GetRobotsTxtFactory(homepageId, culture, true);

            content = response.Content?.RobotsContent;
        }
        else
        {
            var fusionCache = fusionCacheProvider.GetCache(CachingConstants.SiteApi.CacheName);

            var data = await fusionCache.GetOrSetAsync<RobotsData?>(
                CacheKeyExtensions.GetRobotsKey(homepageId, culture),
                async (ctx, ct) =>
                {
                    var response = await GetRobotsTxtFactory(homepageId, culture, false, ct);

                    if (response is { IsSuccessful: true, Content: not null })
                    {
                        return response.Content;
                    }

                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(siteApiCachingOptions.Value.Default.NullDuration));

                    return null;
                },
                tags: [CachingConstants.SiteApi.Tags.Robots, homepageId.ToString(), culture, siteSettingsId.ToString()]);

            content = data?.RobotsContent;
        }

        if (content is null)
        {
            return TypedResults.NotFound();
        }

        var siteSettings = await pagesService.GetPage(siteResolutionContext.Site.SiteSettingsId) as ApiSiteSettings;
        var siteDomain = siteResolutionContext.Site.Domains.First();
        var canonicalDomain = siteSettings?.Properties.CanonicalDomainOverride ?? $"{siteDomain.Scheme}://{siteDomain.Domain}{siteDomain.Path}";

        var sitemapUrls = new List<Uri> { new(new(canonicalDomain), "sitemap.xml") };

        var alternateSites = await siteResolutionService.GetAlternateSites(siteResolutionContext.Site);

        var alternateCanonicalTasks = alternateSites
            .Select(x => pagesService.GetPage(siteResolutionContext.Site.SiteSettingsId, isPreview, x))
            .ToArray();

        await Task.WhenAll(alternateCanonicalTasks);

        var alternateSitemapUrls = alternateCanonicalTasks
            .Select(x => x.Result as ApiSiteSettings)
            .Zip(alternateSites)
            .Select(x =>
            {
                var alternateDomain = x.Second.Domains.First();

                return x.First?.Properties.CanonicalDomainOverride ?? $"{alternateDomain.Scheme}://{alternateDomain.Domain}{alternateDomain.Path}";
            })
            .Select(x => new Uri(new(x), "sitemap.xml"));

        sitemapUrls.AddRange(alternateSitemapUrls);

        var robotsSitemaps = string.Join('\n', sitemapUrls.Select(x => $"Sitemap: {x}"));

        return TypedResults.Ok(new RobotsTxt
        {
            Content = $"{content}\n{robotsSitemaps}"
        });

        async Task<IApiResponse<RobotsData>> GetRobotsTxtFactory(Guid factoryHome, string factoryCulture, bool factoryPreview, CancellationToken cancellationToken = default)
        {
            var response = await robotsApi.GetRobots(factoryHome, factoryCulture, factoryPreview, cancellationToken);

            return response is { IsSuccessful: false, StatusCode: not HttpStatusCode.NotFound }
                ? throw new SiteApiException((int)response.StatusCode, response.ReasonPhrase)
                : response;
        }
    }
}
