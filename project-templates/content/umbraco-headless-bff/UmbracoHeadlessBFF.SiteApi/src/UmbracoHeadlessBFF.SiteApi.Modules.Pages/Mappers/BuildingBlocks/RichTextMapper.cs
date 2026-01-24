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

        var linksTask = ProcessLinks(doc);
        ProcessMedia(doc);

        await linksTask;

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
                CleanUpLinkAttributes(link);
                continue;
            }

            switch (entityType)
            {
                case "Content":
                    var contentId = link.GetAttributeValue("data-destination-id", string.Empty);
                    if (Guid.TryParse(contentId, out var contentGuid))
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
                    break;

                case "Media":
                    var href = link.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        link.SetAttributeValue("href", $"{href}{anchor}");
                    }
                    break;
            }

            CleanUpLinkAttributes(link);
        }
    }

    private void ProcessMedia(HtmlDocument doc)
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
            }
        }
    }

    private static void CleanUpLinkAttributes(HtmlNode node)
    {
        node.Attributes.Remove("data-link-type");
        node.Attributes.Remove("data-anchor");
        node.Attributes.Remove("data-router-slot");
        node.Attributes.Remove("data-destination-id");
        node.Attributes.Remove("data-destination-type");
        node.Attributes.Remove("data-start-item-path");
        node.Attributes.Remove("data-start-item-id");
    }
}
