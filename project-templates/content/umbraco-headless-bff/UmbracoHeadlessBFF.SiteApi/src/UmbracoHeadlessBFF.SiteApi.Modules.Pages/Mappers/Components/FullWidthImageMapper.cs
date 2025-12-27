using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.Components;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Components;

internal sealed class FullWidthImageMapper : IComponentMapper
{
    private readonly IResponsiveImageMapper _responsiveImageMapper;

    public FullWidthImageMapper(IResponsiveImageMapper responsiveImageMapper)
    {
        _responsiveImageMapper = responsiveImageMapper;
    }

    public bool CanMap(string type) => type == DeliveryApiConstants.ElementTypes.ApiFullWidthImage;

    public async Task<IComponent?> Map(IApiElement? model, IApiElement? settings)
    {
        if (model is not ApiFullWidthImage apiModel)
        {
            return null;
        }

        var image = apiModel.Properties.Image?.Content;

        return new FullWidthImage
        {
            Id = apiModel.Id,
            ContentType = apiModel.ContentType,
            Image = await _responsiveImageMapper.Map(image)
        };
    }
}
