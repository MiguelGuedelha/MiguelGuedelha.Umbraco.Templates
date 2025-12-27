using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components.Compositions;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components;

public sealed record ApiFullWidthImage : IApiElement<ApiFullWidthImageProperties>
{
    public required Guid Id { get; init; }
    public string ContentType => DeliveryApiConstants.ElementTypes.ApiFullWidthImage;
    public required ApiFullWidthImageProperties Properties { get; init; }
}

public sealed record ApiFullWidthImageProperties : IApiJumpMenuConfiguration
{
    public string? JumpMenuHeading { get; init; }
    public string? JumpMenuAnchorId { get; init; }
    public ApiBlockSingleItem<ApiResponsiveImage>? Image { get; init; }
}
