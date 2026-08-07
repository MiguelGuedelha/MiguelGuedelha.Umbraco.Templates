using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.Links;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.BuildingBlocks;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;

internal interface ILinkMapper : IMapper<ApiLink, Link>
{
}

internal sealed class LinkMapper : ILinkMapper
{
    private readonly SiteResolutionContext _siteResolutionContext;
    private readonly LinkService _linkService;

    private const string PlaceholderDomain = "https://example.com/";

    public LinkMapper(SiteResolutionContext siteResolutionContext, LinkService linkService)
    {
        _siteResolutionContext = siteResolutionContext;
        _linkService = linkService;
    }

    public async Task<Link?> Map(ApiLink? model)
    {
        if (model is null)
        {
            return null;
        }

        var placeholderUrl = new Uri($"{PlaceholderDomain}{model.QueryString}");

        switch (model.LinkType)
        {
            case ApiLinkType.Content:
                var link = await _linkService.ResolveLink(model.DestinationId!.Value, model.Culture);

                if (link is null)
                {
                    return null;
                }

                var contentBuilder = new UriBuilder(link.Url)
                {
                    Query = GetJoinedQuery(placeholderUrl, link.Url.Query),
                    Fragment = GetPriorityFragment(placeholderUrl, link.Url.Fragment)
                };

                return new()
                {
                    Href = contentBuilder.Uri.ToString(),
                    Target = model.Target,
                    Title = model.Title
                };

            case ApiLinkType.Media:

                if (string.IsNullOrWhiteSpace(model.Url))
                {
                    return null;
                }

                var mediaBuilder = new UriBuilder(model.Url)
                {
                    Query = placeholderUrl.Query,
                    Fragment = placeholderUrl.Fragment
                };

                return new()
                {
                    Href = mediaBuilder.Uri.ToString(),
                    Target = model.Target,
                    Title = model.Title,
                    IsFile = true
                };

            case ApiLinkType.External:
            default:
                if (model.Url?.StartsWith("tel:") is true
                    || model.Url?.StartsWith("mailto:") is true
                    || model.Url?.StartsWith('#') is true)
                {
                    return new()
                    {
                        Target = null,
                        Href = model.Url,
                        Title = model.Title
                    };
                }

                if (string.IsNullOrWhiteSpace(model.Url) && model.QueryString?.StartsWith('#') is true)
                {
                    return new()
                    {
                        Target = null,
                        Href = model.QueryString,
                        Title = model.Title
                    };
                }

                if (!string.IsNullOrWhiteSpace(model.Url))
                {
                    return new()
                    {
                        Target = model.Target,
                        Href = $"{model.Url}{model.QueryString}",
                        Title = model.Title,
                    };
                }

                break;
        }

        return null;
    }

    private static string GetJoinedQuery(Uri placeholderUri, string query)
    {
        var linkQuery = query.Replace("?", string.Empty);
        var modelQuery = placeholderUri.Query.Replace("?", string.Empty);

        var hasQuery = !string.IsNullOrWhiteSpace(linkQuery) || !string.IsNullOrWhiteSpace(modelQuery);
        var hasBothQueries = !string.IsNullOrWhiteSpace(linkQuery) && string.IsNullOrWhiteSpace(modelQuery);

        return $"{(hasQuery ? "?" : string.Empty)}{linkQuery}{(hasBothQueries ? "&" : string.Empty)}{modelQuery}";
    }

    private static string GetPriorityFragment(Uri placeholderUrl, string fragment)
    {
        var modelFragment = placeholderUrl.Fragment.Replace("#", string.Empty);

        if (modelFragment.Length > 0)
        {
            return placeholderUrl.Fragment;
        }

        var linkFragment = fragment.Replace("#", string.Empty);

        return linkFragment.Length > 0 ? linkFragment : string.Empty;
    }
}
