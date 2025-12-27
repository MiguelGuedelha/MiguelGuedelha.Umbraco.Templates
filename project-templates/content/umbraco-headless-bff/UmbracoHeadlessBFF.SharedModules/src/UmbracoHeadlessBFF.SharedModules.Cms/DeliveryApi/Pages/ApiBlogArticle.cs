using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages.Compositions;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;

public sealed record ApiBlogArticle : IApiContent<ApiBlogArticleProperties>
{
    public required Guid Id { get; init; }
    public string ContentType => DeliveryApiConstants.ContentTypes.ApiBlogArticle;
    public required string Name { get; init; }
    public DateTime CreateDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public required ApiContentRoute Route { get; init; }
    public Dictionary<string, ApiContentRoute> Cultures { get; init; } = [];
    public required ApiBlogArticleProperties Properties { get; init; }
}

public sealed record ApiBlogArticleProperties : IApiSeoSettingsProperties, IApiListingSettingsProperties, IApiNavigationSettingsProperties, IApiPageContent
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public IReadOnlyCollection<ApiMediaWithCrops>? MetaImage { get; init; }
    public string? OgType { get; init; }
    public string? OgTitle { get; init; }
    public string? OgDescription { get; init; }
    public IReadOnlyCollection<ApiMediaWithCrops>? OgImage { get; init; }
    public IReadOnlyCollection<string>? RobotsIndexOptions { get; init; }
    public DateTimeOffset? RobotsUnavailableAfter { get; init; }
    public bool? SitemapShow { get; init; }
    public string? SitemapChangeFrequency { get; init; }
    public decimal SitemapPriority { get; init; }
    public DateOnly? SitemapLastModifiedOverwrite { get; init; }
    public bool ExcludeFromSearch { get; init; }
    public string? ListingTitle { get; init; }
    public string? ListingDescription { get; init; }
    public ApiBlockSingleItem<ApiResponsiveImage>? ListingImage { get; init; }
    public bool ShowBreadcrumbs { get; init; }
    public bool ShowInBreadcrumbs { get; init; }
    public string? BreadcrumbNameOverride { get; init; }
    public bool ShowBreadcrumbLink { get; init; }
    public required ApiBlockGrid MainContent { get; init; }
}
