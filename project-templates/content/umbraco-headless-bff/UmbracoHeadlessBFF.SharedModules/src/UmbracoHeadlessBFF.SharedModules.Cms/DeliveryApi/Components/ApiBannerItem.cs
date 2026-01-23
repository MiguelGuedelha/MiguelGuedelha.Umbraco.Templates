using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components.Compositions;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components;

public sealed record ApiBannerItem : IApiElement<ApiBannerItemProperties>
{
    public required Guid Id { get; init; }
    public required string ContentType { get; init; }
    public required ApiBannerItemProperties Properties { get; init; }
}

public sealed record ApiBannerItemProperties : IApiHeadingWithSubHeading, IApiRteDescription
{
    public string? Heading { get; init; }
    public string HeadingSize { get; init; } = "h2";
    public string? SubHeading { get; init; }
    public ApiRichTextItem? Description { get; init; }
    public ApiBlockSingleItem? BackgroundMedia { get; init; }
}
