using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.BuildingBlocks;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;

internal interface ICardMapper :
    IMapper<IEnumerable<ApiCard>, IReadOnlyCollection<Card>>,
    IMapper<ApiCard, Card>
{
}

internal sealed class CardMapper : ICardMapper
{
    private readonly IResponsiveImageMapper _responsiveImageMapper;
    private readonly ILinkMapper _linkMapper;

    public CardMapper(IResponsiveImageMapper responsiveImageMapper, ILinkMapper linkMapper)
    {
        _responsiveImageMapper = responsiveImageMapper;
        _linkMapper = linkMapper;
    }

    public async Task<IReadOnlyCollection<Card>?> Map(IEnumerable<ApiCard>? model)
    {
        if (model is null)
        {
            return null;
        }

        var mapTasks = model.Select(Map).ToArray();

        await Task.WhenAll(mapTasks);

        return mapTasks.Select(x => x.Result).OfType<Card>().ToArray();
    }

    public async Task<Card?> Map(ApiCard? model)
    {
        if (model is null)
        {
            return null;
        }

        var image = model.Properties.Image?.Content;
        var link = model.Properties.Link?.FirstOrDefault();

        return new()
        {
            Heading = model.Properties.Heading,
            HeadingSize = model.Properties.HeadingSize,
            SubHeading = model.Properties.SubHeading,
            Image = await _responsiveImageMapper.Map(image),
            Link = await _linkMapper.Map(link)
        };
    }
}
