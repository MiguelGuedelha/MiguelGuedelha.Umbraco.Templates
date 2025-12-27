using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages.Compositions;

public interface IApiListingSettingsProperties
{
    bool ExcludeFromSearch { get; init; }
    string? ListingTitle { get; init; }
    string? ListingDescription { get; init; }
    ApiBlockSingleItem<ApiResponsiveImage>? ListingImage { get; init; }
}
