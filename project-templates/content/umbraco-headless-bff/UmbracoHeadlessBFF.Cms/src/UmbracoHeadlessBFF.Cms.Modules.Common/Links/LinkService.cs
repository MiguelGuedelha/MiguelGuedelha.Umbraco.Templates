using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using UmbracoHeadlessBFF.Cms.Modules.Common.Umbraco.Models;
using UmbracoHeadlessBFF.SharedModules.Common.Collections;
using UmbracoHeadlessBFF.SharedModules.Common.Strings;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Links;

public sealed class LinkService
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IDomainCache _domainCache;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public LinkService(
        IPublishedContentCache publishedContentCache,
        IPublishedUrlProvider publishedUrlProvider,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDomainCache domainCache,
        IVariationContextAccessor variationContextAccessor)
    {
        _publishedContentCache = publishedContentCache;
        _publishedUrlProvider = publishedUrlProvider;
        _domainCache = domainCache;
        _variationContextAccessor = variationContextAccessor;
        _documentNavigationQueryService = documentNavigationQueryService;
    }

    public Uri? GetUriByContentId(Guid linkId, string culture, string? domain, bool preview)
    {
        _variationContextAccessor.VariationContext = new(culture);
        var item = _publishedContentCache.GetById(preview, linkId);

        if (item is null)
        {
            return null;
        }

        return preview
            ? GetPreviewUrl(item, culture)
            : GetLiveUrl(item, domain, culture);
    }

    private Uri? GetLiveUrl(IPublishedContent item, string? domain, string culture)
    {
        var domainUri = string.IsNullOrWhiteSpace(domain)
            ? null
            : new Uri(domain.StartsWith("http") ? domain : $"https://{domain}");

        var route = _publishedUrlProvider.GetUrl(item, UrlMode.Absolute, culture: culture, current: domainUri);

        if (domainUri is null)
        {
            return route.Equals("#") ? null : new(route);
        }

        var routeUri = new Uri(route);

        if (routeUri.Authority.Equals(domainUri.Authority))
        {
            return routeUri;
        }

        var routeWithMatchingAuthority = _publishedUrlProvider
            .GetOtherUrls(item.Id)
            .FirstOrDefault(x => (x.Url?.Authority.Equals(domainUri.Authority) ?? false) && (x.Culture?.Equals(culture) ?? false));

        return routeWithMatchingAuthority?.Url ?? routeUri;
    }

    private Uri? GetPreviewUrl(IPublishedContent item, string culture)
    {
        Domain? domain;
        bool parsedDomain;
        Uri? uri;

        if (item is Home home)
        {
            domain = _domainCache.GetAssigned(home.Id).FirstOrDefault(x => x.Culture == culture);

            if (domain is null)
            {
                return null;
            }

            parsedDomain = Uri.TryCreate(domain.Name.EnsureHttpScheme(), UriKind.Absolute, out uri);

            return parsedDomain ? uri : null;
        }

        var hasKeys = _documentNavigationQueryService.TryGetAncestorsKeys(item.Key, out var keys);

        if (!hasKeys)
        {
            return null;
        }

        var ancestors = keys
            .Select(x => _publishedContentCache.GetById(true, x))
            .WhereNotNull()
            .OrderBy(x => x.Level)
            .Where(x => x.ContentType.Alias != SiteGrouping.ModelTypeAlias)
            .ToArray();

        var homeContent = ancestors.FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias);

        if (homeContent is null)
        {
            return null;
        }

        home = (homeContent as Home)!;

        domain = _domainCache.GetAssigned(home.Id).FirstOrDefault(x => x.Culture == culture);

        if (domain is null)
        {
            return null;
        }

        var nodesExcHome = ancestors
            .Where(x => x.ContentType.Alias != Home.ModelTypeAlias)
            .Select(x => x.UrlSegment)
            .Append(item.UrlSegment)
            .WhereNotNull()
            .ToArray();

        parsedDomain = Uri.TryCreate(domain.Name.EnsureHttpScheme().CombineUri(nodesExcHome), UriKind.Absolute, out uri);

        return parsedDomain ? uri : null;
    }
}
