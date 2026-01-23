namespace UmbracoHeadlessBFF.SharedModules.Cms.Links;

public sealed record RedirectLink
{
    public required string? Location { get; init; }
    public required int? StatusCode { get; init; }
}
