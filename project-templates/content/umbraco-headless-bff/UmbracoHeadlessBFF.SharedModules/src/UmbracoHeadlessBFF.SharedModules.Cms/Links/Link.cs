namespace UmbracoHeadlessBFF.SharedModules.Cms.Links;

public sealed record Link
{
    public required Uri Url { get; init; }
}
