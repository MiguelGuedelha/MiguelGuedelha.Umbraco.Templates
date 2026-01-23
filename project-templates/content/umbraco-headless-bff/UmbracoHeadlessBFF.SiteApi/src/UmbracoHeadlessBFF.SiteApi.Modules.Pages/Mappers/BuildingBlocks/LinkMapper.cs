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
                        Href = link.Path,
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
                        Href = model.Url,
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

        if (string.IsNullOrWhiteSpace(model.QueryString))
        {
            return new()
            {
                Target = model.Target,
                Href = uriBuilder.Uri.ToString(),
                Title = model.Title,
                IsFile = model.LinkType == ApiLinkType.Media
            };
        }

        var queryStart = model.QueryString.IndexOf("?", StringComparison.OrdinalIgnoreCase);
        var anchorStart = model.QueryString.IndexOf("#", StringComparison.OrdinalIgnoreCase);

        var querySegment = (queryStart, anchorStart) switch
        {
            (0, -1) => model.QueryString,
            (0, > 0) => model.QueryString[..anchorStart],
            (> 0, -1) => model.QueryString[queryStart..],
            (> 0, > 0) when queryStart > anchorStart => model.QueryString[queryStart..],
            (> 0, > 0) when queryStart < anchorStart => model.QueryString[queryStart..anchorStart],
            _ => null
        };

        if (querySegment is not null)
        {
            uriBuilder.Query = querySegment;
        }

        var anchorSegment = (anchorStart, queryStart) switch
        {
            (0, -1) => model.QueryString,
            (0, > 0) => model.QueryString[..queryStart],
            (> 0, -1) => model.QueryString[anchorStart..],
            (> 0, > 0) when anchorStart > queryStart => model.QueryString[anchorStart..],
            (> 0, > 0) when anchorStart < queryStart => model.QueryString[anchorStart..queryStart],
            _ => null
        };

        if (anchorSegment is not null)
        {
            uriBuilder.Fragment = anchorSegment;
        }

        return new()
        {
            Target = model.Target,
            Href = uriBuilder.Uri.ToString(),
            Title = model.Title,
            IsFile = model.LinkType == ApiLinkType.Media
        };
    }
}
