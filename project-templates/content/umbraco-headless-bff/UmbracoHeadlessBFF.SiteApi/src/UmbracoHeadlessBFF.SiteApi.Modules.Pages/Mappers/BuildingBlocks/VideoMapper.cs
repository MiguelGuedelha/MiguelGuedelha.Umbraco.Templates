using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.BuildingBlocks;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;

internal interface IVideoMapper : IMapper<ApiMediaWithCrops, Video>
{
}

internal sealed class VideoMapper : IVideoMapper
{
    public Task<Video?> Map(ApiMediaWithCrops? model)
    {
        if (model is null)
        {
            return Task.FromResult<Video?>(null);
        }

        return Task.FromResult<Video?>(new()
        {
            Src = model.Url,
            Type = $"video/{model.Extension}",
        });
    }
}
