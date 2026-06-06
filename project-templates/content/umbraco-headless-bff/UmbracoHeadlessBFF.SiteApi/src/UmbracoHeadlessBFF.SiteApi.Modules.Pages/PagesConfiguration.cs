using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Converters;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Endpoints;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Components;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Layouts;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.Pages;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages;

public static class PagesConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddPagesModule()
        {
            var services = builder.Services;

            services.AddTransient<IPagesService, PagesService>();

            // Building Block Mappers
            services
                .AddTransient<ILinkMapper, LinkMapper>()
                .AddTransient<ICardMapper, CardMapper>()
                .AddTransient<IImageMapper, ImageMapper>()
                .AddTransient<IVideoMapper, VideoMapper>()
                .AddTransient<IMediaBlockMapper, MediaBlockMapper>()
                .AddTransient<IEmbedVideoMapper, EmbedVideoMapper>()
                .AddTransient<IMediaLibraryVideoMapper, MediaLibraryVideoMapper>()
                .AddTransient<IResponsiveImageMapper, ResponsiveImageMapper>()
                .AddTransient<INavigationLinkMapper, NavigationLinkMapper>()
                .AddTransient<ILinkWithSublinksMapper, LinkWithSublinksMapper>()
                .AddTransient<IHeadingWithLinksMapper, HeadingWithLinksMapper>()
                .AddTransient<IHeadingWithSocialLinksMapper, HeadingWithSocialLinksMapper>()
                .AddTransient<IRichTextMapper, RichTextMapper>();

            // Component Mappers
            services
                .AddTransient<IComponentMapper, SpotlightMapper>()
                .AddTransient<IComponentMapper, GalleryMapper>()
                .AddTransient<IComponentMapper, RichTextSectionMapper>()
                .AddTransient<IComponentMapper, SectionHeaderMapper>()
                .AddTransient<IComponentMapper, FullWidthImageMapper>()
                .AddTransient<IComponentMapper, CarouselMapper>()
                .AddTransient<IComponentMapper, BannerMapper>()
                .AddTransient<IComponentMapper, BlogListingMapper>();

            services.AddTransient<IComponentMapper, FallbackComponentMapper>();

            // LayoutMappers
            services
                .AddTransient<ILayoutMapper, OneColumnMapper>();

            services.AddTransient<ILayoutMapper, FallbackLayoutMapper>();

            // Page related mappers
            services
                .AddTransient<BasePageMapper>()
                .AddTransient<ISeoMapper, SeoMapper>()
                .AddTransient<ISiteSettingsMapper, SiteSettingsMapper>();

            // Page Mappers
            services
                .AddTransient<IPageMapper, HomeMapper>()
                .AddTransient<IPageMapper, SiteSearchMapper>()
                .AddTransient<IPageMapper, StandardContentMapper>()
                .AddTransient<IPageMapper, BlogArticleMapper>()
                .AddTransient<IPageMapper, NotFoundMapper>();

            services.AddTransient<IPageMapper, FallbackPageMapper>();
        }
    }

    extension(IList<JsonConverter> converters)
    {
        public void AddPagesConverters()
        {
            converters.Add(new ComponentConverter());
            converters.Add(new MediaBlockConverter());
            converters.Add(new LayoutConverter());
            converters.Add(new PageConverter());
        }
    }

    extension(RouteGroupBuilder builder)
    {
        public void MapPagesEndpoints()
        {
            builder
                .MapGroup("/pages")
                .WithTags("Pages")
                .MapGetPageByIdOrPath()
                .MapGetNotFoundPage()
                .MapGetSitemap()
                .MapGetRobots()
                .MapGetDictionary();
        }
    }
}
