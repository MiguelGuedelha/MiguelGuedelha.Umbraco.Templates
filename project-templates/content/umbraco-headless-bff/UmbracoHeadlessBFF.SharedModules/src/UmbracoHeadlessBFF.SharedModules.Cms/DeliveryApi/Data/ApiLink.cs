using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

public sealed record ApiLink
{
    public string? Url { get; init; }
    public string? QueryString { get; init; }
    public string? Title { get; init; }
    public string? Target { get; init; }
    public Guid? DestinationId { get; init; }
    public string? DestinationType { get; init; }
    public ApiContentRoute? Route { get; init; }
    public ApiLinkType LinkType { get; init; }
    public string? Culture { get; init; }
}
