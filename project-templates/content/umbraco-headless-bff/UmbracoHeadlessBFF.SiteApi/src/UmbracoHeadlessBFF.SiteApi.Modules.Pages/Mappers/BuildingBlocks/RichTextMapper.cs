using HtmlAgilityPack;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;

internal interface IRichTextMapper : IMapper<ApiRichTextItem, string>
{
}

internal sealed class RichTextMapper : IRichTextMapper
{
    private readonly ILinkMapper _linkMapper;

    public RichTextMapper(ILinkMapper linkMapper)
    {
        _linkMapper = linkMapper;
    }

    public async Task<string?> Map(ApiRichTextItem? model)
    {
        if (string.IsNullOrWhiteSpace(model?.Markup))
        {
            return null;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(model.Markup);

        await ProcessLinks(doc);
        await ProcessImages(doc);

        return doc.DocumentNode.InnerHtml;
    }

    private async Task ProcessLinks(HtmlDocument doc)
    {
        var links = doc.DocumentNode.SelectNodes("//a");

        if (links is null or [])
        {
            return;
        }

        foreach (var link in links)
        {
            var entityType = link.GetAttributeValue("data-link-type", string.Empty);
            var anchor = link.GetAttributeValue("data-anchor", string.Empty);

            if (string.IsNullOrWhiteSpace(entityType))
            {
                link.Attributes.Remove("data-link-type");
                continue;
            }

            switch (entityType)
            {
                case "Content":
                    var contentId = link.GetAttributeValue("data-destination-id", string.Empty);
                    if (!string.IsNullOrWhiteSpace(contentId) && Guid.TryParse(contentId, out var contentGuid))
                    {
                        var contentLink = await _linkMapper.Map(new()
                        {
                            DestinationId = contentGuid,
                            LinkType = ApiLinkType.Content,
                            QueryString = anchor
                        });

                        if (contentLink is not null)
                        {
                            link.SetAttributeValue("href", contentLink.Href);
                        }
                    }
                    link.Attributes.Remove("data-content-id");
                    break;

                case "Media":
                    var href = link.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        var pathAndQuery = href.Split('?');
                        var cleanAnchor = anchor.Replace("?", "");

                        var hasAnchor = string.IsNullOrWhiteSpace(cleanAnchor);

                        var mediaLink = await _linkMapper.Map(new()
                        {
                            Url = pathAndQuery[0],
                            LinkType = ApiLinkType.Media,
                            QueryString = pathAndQuery.Length == 2 ? $"{pathAndQuery[1]}{(hasAnchor ? $"&{cleanAnchor}" : "")}" : cleanAnchor
                        });

                        if (mediaLink is not null)
                        {
                            link.SetAttributeValue("href", mediaLink.Href);
                        }
                    }
                    break;
            }

            link.Attributes.Remove("data-anchor");
            link.Attributes.Remove("data-link-type");
        }
    }

    private async Task ProcessImages(HtmlDocument doc)
    {
        var images = doc.DocumentNode.SelectNodes("//img");

        if (images is null or [])
        {
            return;
        }

        foreach (var image in images)
        {
            var url = image.GetAttributeValue("src", string.Empty);

            if (string.IsNullOrWhiteSpace(url))
            {
                image.Attributes.Remove("src");
                continue;
            }

            var pathAndQuery = url.Split('?');

            var mediaLink = await _linkMapper.Map(new()
            {
                Url = pathAndQuery[0],
                LinkType = ApiLinkType.Media,
                QueryString = pathAndQuery.Length == 2 ? pathAndQuery[1] : null
            });

            if (mediaLink is not null)
            {
                image.SetAttributeValue("src", mediaLink.Href);
            }
        }
    }
}
