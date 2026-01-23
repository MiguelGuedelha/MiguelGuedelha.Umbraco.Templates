using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components.Compositions;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components;

public sealed record ApiSpotlight : IApiElement<ApiSpotlightProperties>
{
    public required Guid Id { get; init; }
    public string ContentType => DeliveryApiConstants.ElementTypes.ApiSpotlight;
    public required ApiSpotlightProperties Properties { get; init; }
}

public sealed record ApiSpotlightProperties : IApiJumpMenuConfiguration, IApiHeading, IApiRteDescription
{
    public string? JumpMenuHeading { get; init; }
    public string? JumpMenuAnchorId { get; init; }
    public string? Heading { get; init; }
    public string HeadingSize { get; init; } = "h2";
    public ApiRichTextItem? Description { get; init; }
    public IReadOnlyCollection<ApiLink>? Cta { get; init; }
    public ApiBlockSingleItem? Media { get; init; }
}
