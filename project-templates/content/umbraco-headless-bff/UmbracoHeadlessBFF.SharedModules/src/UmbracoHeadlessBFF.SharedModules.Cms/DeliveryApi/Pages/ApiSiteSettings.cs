using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;

public sealed record ApiSiteSettings : IApiContent<ApiSiteSettingsProperties>
{
    public required Guid Id { get; init; }
    public string ContentType => DeliveryApiConstants.ContentTypes.ApiSiteSettings;
    public required string Name { get; init; }
    public DateTime CreateDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public required ApiContentRoute Route { get; init; }
    public Dictionary<string, ApiContentRoute> Cultures { get; init; } = [];
    public required ApiSiteSettingsProperties Properties { get; init; }
}

public sealed record ApiSiteSettingsProperties
{
    public IReadOnlyCollection<ApiMediaWithCrops>? HeaderLogo { get; init; }
    public IReadOnlyCollection<ApiLink>? HeaderQuickLinks { get; init; }
    public ApiBlockList<ApiMainNavigationLink>? HeaderNavigation { get; init; }
    public IReadOnlyCollection<ApiMediaWithCrops>? FooterLogo { get; init; }
    public ApiBlockList<ApiHeadingWithLinks>? FooterLinks { get; init; }
    public ApiBlockSingleItem<ApiHeadingWithSocialLinks>? FooterSocialLinks { get; init; }
    public IReadOnlyCollection<ApiLink>? FooterFootnoteLinks { get; init; }
    public string? Copyright { get; init; }
    public IReadOnlyCollection<ApiContentReference>? SearchPage { get; init; }
    public string? CanonicalDomainOverride { get; init; }
    public string? PageTitlePrefix { get; init; }
}
