namespace UmbracoHeadlessBFF.SharedModules.Cms.Options;

public sealed record CmsServiceOptions
{
    public const string SectionName = "services:Cms";
    public required CmsServiceParameters Parameters { get; init; }
}

public sealed record CmsServiceParameters
{
    public required string DeliveryApiKey { get; init; }
}
