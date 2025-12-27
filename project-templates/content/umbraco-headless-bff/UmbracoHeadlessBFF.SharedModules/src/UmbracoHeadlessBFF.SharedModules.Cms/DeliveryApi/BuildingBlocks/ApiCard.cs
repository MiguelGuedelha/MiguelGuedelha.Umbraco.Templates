using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components.Compositions;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;

public sealed record ApiCard : IApiElement<ApiCardProperties>
{
    public required Guid Id { get; init; }
    public string ContentType => DeliveryApiConstants.ElementTypes.ApiCard;
    public required ApiCardProperties Properties { get; init; }
}

public sealed record ApiCardProperties : IApiHeadingWithSubHeading
{
    public string? Heading { get; init; }
    public string HeadingSize { get; init; } = "h3";
    public string? SubHeading { get; init; }
    public ApiBlockSingleItem<ApiResponsiveImage>? Image { get; init; }
    public IReadOnlyCollection<ApiLink>? Link { get; init; }
}
