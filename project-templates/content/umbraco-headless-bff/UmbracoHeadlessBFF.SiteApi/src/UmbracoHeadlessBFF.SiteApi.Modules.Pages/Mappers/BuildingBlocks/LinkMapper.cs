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

        UriBuilder uriBuilder;

        var query = string.IsNullOrWhiteSpace(model.QueryString) ? string.Empty : model.QueryString;

        switch (model.LinkType)
        {
            case ApiLinkType.Content:
                var link = await _linkService.ResolveLink(model.DestinationId!.Value);

                if (link is null)
                {
                    return null;
                }

                if (_siteResolutionContext.Site.Domains.First().Domain == link.Authority)
                {
                    return new()
                    {
                        Href = $"{link.Path}{query}",
                        Title = model.Title,
                        Target = model.Target
                    };
                }

                var hostPortSplit = link.Authority.Split(":");

                uriBuilder = new(hostPortSplit[0])
                {
                    Path = link.Path,
                    Port = hostPortSplit.Length > 1 ? int.Parse(hostPortSplit[1]) : -1
                };

                break;

            case ApiLinkType.Media:

                if (string.IsNullOrWhiteSpace(model.Url))
                {
                    return null;
                }

                uriBuilder = new(model.Url);

                break;
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
                        Title = model.Title,
                    };
                }

                if (model.Url?.StartsWith('/') is true)
                {
                    return new()
                    {
                        Target = model.Target,
                        Href = $"{model.Url}{query}",
                        Title = model.Title,
                    };
                }

                if (string.IsNullOrWhiteSpace(model.Url))
                {
                    return null;
                }

                uriBuilder = new(model.Url);
                break;
        }

        return new()
        {
            Target = model.Target,
            Href = $"{uriBuilder.Uri.ToString()}{query}",
            Title = model.Title,
            IsFile = model.LinkType == ApiLinkType.Media
        };
    }
}
