using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Pages;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.Pages;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Pages;

internal interface ISiteSettingsMapper : IMapper<ApiSiteSettings, SiteSettings>
{
}

internal sealed class SiteSettingsMapper : ISiteSettingsMapper
{
    private readonly IImageMapper _imageMapper;
    private readonly ILinkMapper _linkMapper;
    private readonly INavigationLinkMapper _navigationLinkMapper;
    private readonly IHeadingWithLinksMapper _headingWithLinksMapper;
    private readonly IHeadingWithSocialLinksMapper _headingWithSocialLinksMapper;

    public SiteSettingsMapper(IImageMapper imageMapper, ILinkMapper linkMapper, INavigationLinkMapper navigationLinkMapper,
        IHeadingWithLinksMapper headingWithLinksMapper, IHeadingWithSocialLinksMapper headingWithSocialLinksMapper)
    {
        _imageMapper = imageMapper;
        _linkMapper = linkMapper;
        _navigationLinkMapper = navigationLinkMapper;
        _headingWithLinksMapper = headingWithLinksMapper;
        _headingWithSocialLinksMapper = headingWithSocialLinksMapper;
    }

    public async Task<SiteSettings?> Map(ApiSiteSettings? model)
    {
        if (model is null)
        {
            return null;
        }

        var headerLogo = model.Properties.HeaderLogo?.FirstOrDefault();

        var quickLinkTasks = model.Properties.HeaderQuickLinks?.Select(x => _linkMapper.Map(x)).ToArray() ?? [];
        await Task.WhenAll(quickLinkTasks);

        var navigationTasks = model.Properties.HeaderNavigation?.Items
            .Select(x => _navigationLinkMapper.Map(x.Content)).ToArray() ?? [];
        await Task.WhenAll(quickLinkTasks);

        var footerLogo = model.Properties.FooterLogo?.FirstOrDefault();

        var headingWithLinksTasks = model.Properties.FooterLinks?.Items
            .Select(x => _headingWithLinksMapper.Map(x.Content)).ToArray() ?? [];
        await Task.WhenAll(headingWithLinksTasks);

        var socialLinksData = model.Properties.FooterSocialLinks;
        var socialLinks = await _headingWithSocialLinksMapper.Map(socialLinksData?.Content);

        var footnoteLinks = model.Properties.FooterFootnoteLinks?
            .Select(x => _linkMapper.Map(x)).ToArray() ?? [];
        await Task.WhenAll(footnoteLinks);

        return new()
        {
            SearchPage = model.Properties.SearchPage?.FirstOrDefault()?.Route.Path,
            Header = new()
            {
                Logo = headerLogo is not null ? (await _imageMapper.Map(headerLogo))?.Src : null,
                QuickLinks = quickLinkTasks.Select(x => x.Result).OfType<Link>().ToArray(),
                Navigation = navigationTasks.Select(x => x.Result).OfType<NavigationLink>().ToArray()
            },
            Footer = new()
            {
                Logo = footerLogo is not null ? (await _imageMapper.Map(footerLogo))?.Src : null,
                Links = headingWithLinksTasks.Select(x => x.Result).OfType<HeadingWithLinks>().ToArray(),
                SocialLinks = socialLinks,
                Copyright = model.Properties.Copyright,
                FootnoteLinks = footnoteLinks.Select(x => x.Result).OfType<Link>().ToArray()
            },

        };
    }
}
