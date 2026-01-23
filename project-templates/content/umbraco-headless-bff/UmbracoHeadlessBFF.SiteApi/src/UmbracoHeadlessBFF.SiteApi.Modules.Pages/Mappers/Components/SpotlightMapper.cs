using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Components;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.Components;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Components;

internal sealed class SpotlightMapper : IComponentMapper
{
    private readonly ILinkMapper _linkMapper;
    private readonly IMediaBlockMapper _mediaBlockMapper;
    private readonly IRichTextMapper _richTextMapper;

    public SpotlightMapper(ILinkMapper linkMapper, IMediaBlockMapper mediaBlockMapper, IRichTextMapper richTextMapper)
    {
        _linkMapper = linkMapper;
        _mediaBlockMapper = mediaBlockMapper;
        _richTextMapper = richTextMapper;
    }

    public bool CanMap(string type) => type == DeliveryApiConstants.ElementTypes.ApiSpotlight;

    public async Task<IComponent?> Map(IApiElement? model, IApiElement? settings)
    {
        if (model is not ApiSpotlight apiModel)
        {
            return null;
        }

        var media = apiModel.Properties.Media?.Content;

        var cta = apiModel.Properties.Cta?.FirstOrDefault();
        var description = apiModel.Properties.Description;

        return new Spotlight
        {
            Id = apiModel.Id,
            ContentType = apiModel.ContentType,
            Heading = apiModel.Properties.Heading,
            HeadingSize = apiModel.Properties.HeadingSize,
            Description = await _richTextMapper.Map(description),
            Media = await _mediaBlockMapper.Map(media),
            Cta = await _linkMapper.Map(cta)
        };
    }
}
