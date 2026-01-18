using System.Net;
using Microsoft.Extensions.Options;
using Refit;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;
using UmbracoHeadlessBFF.SharedModules.Cms.Links;
using UmbracoHeadlessBFF.SharedModules.Cms.SiteResolution;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Strings;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Errors;
using ZiggyCreatures.Caching.Fusion;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages;

internal interface IPagesService
{
    Task<IApiContent?> GetPage(Guid id, bool? isPreview = null, SiteDefinition? site = null);
    Task<IApiContent?> GetPage(string path, bool? isPreview = null, SiteDefinition? site = null);
    Task<PagedApiContent?> GetPages(int skip = 0, int take = 10, ContentFetchType? fetch = null,
        IReadOnlyCollection<ContentFilterType>? filter = null, ContentSortType? sort = null, string? startItem = null);
}

internal sealed class PagesService : IPagesService
{
    private readonly IUmbracoDeliveryApi _umbracoDeliveryApi;
    private readonly SiteResolutionContext _siteResolutionContext;
    private readonly ILinksApi _linksApi;
    private readonly IFusionCache _fusionCache;
    private readonly SiteApiCachingOptions _siteApiCachingOptions;

    private static readonly string s_levelOneExpandFieldsLevel = new FieldsExpandProperties(1).ToString();
    private static readonly string s_defaultExpandFieldsLevel = new FieldsExpandProperties(5).ToString();

    public PagesService(
        IUmbracoDeliveryApi umbracoDeliveryApi,
        SiteResolutionContext siteResolutionContext,
        ILinksApi linksApi,
        IFusionCacheProvider fusionCacheProvider,
        IOptionsSnapshot<SiteApiCachingOptions> siteApiCachingOptions)
    {
        _umbracoDeliveryApi = umbracoDeliveryApi;
        _siteResolutionContext = siteResolutionContext;
        _linksApi = linksApi;
        _fusionCache = fusionCacheProvider.GetCache(CachingConstants.SiteApi.CacheName);
        _siteApiCachingOptions = siteApiCachingOptions.Value;
    }

    public async Task<IApiContent?> GetPage(Guid id, bool? isPreview = null, SiteDefinition? site = null)
    {
        var requestSite = site ?? _siteResolutionContext.Site;
        var homepageId = requestSite.HomepageId;
        var culture = requestSite.CultureInfo;
        var siteSettingsId = requestSite.SiteSettingsId;
        var dictionaryId = requestSite.DictionaryId;

        if (isPreview ?? _siteResolutionContext.IsPreview)
        {
            return (await GetPageByIdFactory(id, true, requestSite)).Content;
        }

        return await _fusionCache.GetOrSetAsync<IApiContent?>(
            CacheKeyExtensions.GetPageByIdKey(homepageId, culture, id),
            async (ctx, ct) =>
            {
                var response = await GetPageByIdFactory(id, false, requestSite, ct);

                if (response.Content is null)
                {
                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(_siteApiCachingOptions.Default.NullDuration));
                }

                return response.Content;
            },
            tags: [CachingConstants.SiteApi.Tags.Pages, id.ToString(), homepageId.ToString(), culture, siteSettingsId.ToString(), dictionaryId.ToString()]);

        async Task<IApiResponse<IApiContent>> GetPageByIdFactory(Guid factoryId, bool factoryPreview, SiteDefinition factorySite, CancellationToken cancellationToken = default)
        {
            var response = await _umbracoDeliveryApi.GetItemById(
                id: factoryId,
                expand: s_defaultExpandFieldsLevel,
                acceptLanguage: factorySite.CultureInfo,
                preview: factoryPreview,
                startItem: factorySite.RootId.ToString(),
                cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            throw new SiteApiException((int)response.StatusCode, response.ReasonPhrase);
        }
    }

    public async Task<IApiContent?> GetPage(string path, bool? isPreview = null, SiteDefinition? site = null)
    {
        var sanitizedPath = path.SanitisePathSlashes();

        var requestSite = site ?? _siteResolutionContext.Site;
        var requestPreview = isPreview ?? _siteResolutionContext.IsPreview;
        var homepageId = requestSite.HomepageId;
        var culture = requestSite.CultureInfo;

        var matchingDomain = requestSite.Domains.FirstOrDefault(x => sanitizedPath.StartsWith(x.Path) && _siteResolutionContext.Domain == x.Domain);

        if (matchingDomain is null)
        {
            return null;
        }

        var deliveryApiPath = requestSite.RootId == requestSite.HomepageId
            ? sanitizedPath.Replace(matchingDomain.Path, "/").SanitisePathSlashes()
            : sanitizedPath.Replace(matchingDomain.Path, requestSite.BasePath).SanitisePathSlashes();

        if (requestPreview)
        {
            var response = await GetPageByPathFactory(deliveryApiPath, requestSite, true);
            return response.Content;
        }

        var redirectPath = sanitizedPath.Replace(matchingDomain.Path, "/");

        var redirect = await _fusionCache.GetOrSetAsync<RedirectLink?>(
            CacheKeyExtensions.GetRedirectKey(homepageId, culture, sanitizedPath),
            async (ctx, ct) =>
            {
                var redirectResponse = await _linksApi.GetRedirect(redirectPath, homepageId, culture, ct);

                if (redirectResponse.Content is null)
                {
                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(_siteApiCachingOptions.Default.NullDuration));
                }

                return redirectResponse.Content;
            },
            tags: [CachingConstants.SiteApi.Tags.Redirects, homepageId.ToString(), culture]);

        if (redirect is not null)
        {
            throw new SiteApiRedirectException(redirect.StatusCode, redirect.Location);
        }

        return await _fusionCache.GetOrSetAsync<IApiContent?>(
            CacheKeyExtensions.GetPageByPathKey(homepageId, culture, sanitizedPath),
            async (ctx, ct) =>
            {
                var response = await GetPageByPathFactory(deliveryApiPath, requestSite, false, ct);

                if (response.Content is null)
                {
                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(_siteApiCachingOptions.Default.NullDuration));
                }

                if (response.Content is { } page)
                {
                    ctx.Tags = ctx.Tags?.Append(page.Id.ToString()).ToArray();
                }

                return response.Content;
            },
            tags: [CachingConstants.SiteApi.Tags.Pages, homepageId.ToString(), culture]);

        async Task<IApiResponse<IApiContent>> GetPageByPathFactory(string factoryPath, SiteDefinition factorySite, bool factoryPreview, CancellationToken cancellationToken = default)
        {
            var response = await _umbracoDeliveryApi.GetItemByPath(
                path: factoryPath,
                expand: s_defaultExpandFieldsLevel,
                acceptLanguage: factorySite.CultureInfo,
                preview: factoryPreview,
                startItem: factorySite.RootId.ToString(),
                cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            throw new SiteApiException((int)response.StatusCode, response.ReasonPhrase);
        }
    }

    public async Task<PagedApiContent?> GetPages(
        int skip = 0,
        int take = 10,
        ContentFetchType? fetch = null,
        IReadOnlyCollection<ContentFilterType>? filter = null,
        ContentSortType? sort = null,
        string? startItem = null)
    {
        var site = _siteResolutionContext.Site;
        var isPreview = _siteResolutionContext.IsPreview;
        var homepageId = _siteResolutionContext.Site.HomepageId;
        var culture = _siteResolutionContext.Site.CultureInfo;

        if (isPreview)
        {
            var response = await GetPagesFactory();
            return response.Content;
        }

        var tags = new List<string> { CachingConstants.SiteApi.Tags.Pages, homepageId.ToString(), culture };

        if (fetch is not null)
        {
            var isGuid = Guid.TryParse(fetch.IdOrPath, out var relatedId);

            var id = isGuid switch
            {
                true => relatedId,
                false => (await GetPage(fetch.IdOrPath))?.Id
            };

            if (id is not null)
            {
                tags.Add(id.Value.ToString());
            }
        }

        var data = await _fusionCache.GetOrSetAsync<PagedApiContent?>(
            CacheKeyExtensions.GetPageListKey(homepageId, culture, skip, take, startItem, fetch, filter, sort),
            async (ctx, ct) =>
            {
                var response = await GetPagesFactory(ct);

                if (response.Content is null)
                {
                    ctx.Options.SetAllDurations(TimeSpan.FromSeconds(_siteApiCachingOptions.Default.NullDuration));
                }

                return response.Content;
            },
            tags: tags);

        return data;

        async Task<IApiResponse<PagedApiContent>> GetPagesFactory(CancellationToken cancellationToken = default)
        {
            var response = await _umbracoDeliveryApi.GetContent(
                fetch?.ToString(),
                filter?.Select(x => x.ToString()),
                sort?.ToString(),
                skip,
                take,
                s_levelOneExpandFieldsLevel,
                acceptLanguage: site.CultureInfo,
                preview: _siteResolutionContext.IsPreview,
                startItem: startItem,
                cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            throw new SiteApiException((int)response.StatusCode, response.ReasonPhrase);
        }
    }
}
