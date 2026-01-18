using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;

public static class CacheKeyExtensions
{
    public static string GetLinkKey(Guid id, string culture) => $"Region:{CachingRegionConstants.Links}:{id}-{culture}";

    public static string GetSitesListKey() => $"Region:{CachingRegionConstants.Sites}:List";

    public static string GetRobotsKey(Guid homeId, string culture) =>
        $"Region:{CachingRegionConstants.Robots}:Site:{homeId}-{culture}";

    public static string GetSitemapKey(Guid homeId, string culture) =>
        $"Region:{CachingRegionConstants.Sitemap}:Site:{homeId}-{culture}";

    public static string GetPageByIdKey(Guid homeId, string culture, Guid pageId) =>
        $"Region:{CachingRegionConstants.Pages}:Site:{homeId}-{culture}:Id:{pageId}";

    public static string GetPageByPathKey(Guid homeId, string culture, string path) =>
        $"Region:{CachingRegionConstants.Pages}:Site:{homeId}-{culture}:Path:{path}";

    public static string GetPageListKey(
        Guid homeId,
        string culture,
        int skip,
        int take,
        string? start,
        ContentFetchType? fetch,
        IReadOnlyCollection<ContentFilterType>? filter,
        ContentSortType? sort)
    {
        var startItemSegment = start ?? "none";
        var fetchSegment = fetch is null ? "no-fetch" : fetch.ToString().Replace(':', '-');
        var filterSegment = filter is null or { Count: 0 } ? "no-filter" : string.Join("-", filter.Select(x => x.ToString()).Order());
        var sortSegment = sort is null ? "no-sort" : sort.ToString().Replace(':', '-');
        var sizeSegment = $"{skip}-{take}";

        return $"Region:{CachingRegionConstants.Pages}:Site:{homeId}-{culture}:List:{startItemSegment}_{sizeSegment}_{fetchSegment}_{filterSegment}_{sortSegment}";
    }

    public static string GetRedirectKey(Guid homeId, string culture, string path) =>
        $"Region:{CachingRegionConstants.Redirects}:Site:{homeId}-{culture}:Path:{path}";
}
