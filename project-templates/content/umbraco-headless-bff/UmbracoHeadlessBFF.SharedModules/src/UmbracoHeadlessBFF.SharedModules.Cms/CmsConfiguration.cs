using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;
using UmbracoHeadlessBFF.SharedModules.Cms.Links;
using UmbracoHeadlessBFF.SharedModules.Cms.Options;
using UmbracoHeadlessBFF.SharedModules.Cms.Preview;
using UmbracoHeadlessBFF.SharedModules.Cms.Robots;
using UmbracoHeadlessBFF.SharedModules.Cms.Sitemap;
using UmbracoHeadlessBFF.SharedModules.Cms.SiteResolution;
using UmbracoHeadlessBFF.SharedModules.Common.ServiceDiscovery;

namespace UmbracoHeadlessBFF.SharedModules.Cms;

public static class CmsConfiguration
{
    private static readonly RefitSettings s_clientSettings = new()
    {
        CollectionFormat = CollectionFormat.Multi,
        ContentSerializer = new SystemTextJsonContentSerializer(new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new ApiElementConverter(),
                new ApiContentConverter()
            },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        })
    };

    extension(WebApplicationBuilder builder)
    {
        public void AddCmsCommonSharedModule()
        {
            // Delivery Api
            builder.Services.Configure<CmsServiceOptions>(builder.Configuration.GetSection(CmsServiceOptions.SectionName));
            builder.Services.AddTransient<DeliveryApiHeadersHandler>();
            builder.Services.AddRefitGeneratedClient<IUmbracoDeliveryApi>(s_clientSettings)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/umbraco/delivery/api/v2");
                })
                .AddHttpMessageHandler<DeliveryApiHeadersHandler>()
                .AddHeaderPropagation();

            // Site Resolution
            builder.Services.AddTransient<DeliveryApiHeadersHandler>();
            builder.Services.AddRefitGeneratedClient<ISiteResolutionApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/api/v1.0/sites");
                })
                .AddHttpMessageHandler<DeliveryApiHeadersHandler>()
                .AddHeaderPropagation();

            // Links
            builder.Services.AddRefitGeneratedClient<ILinksApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/api/v1.0/links");
                })
                .AddHttpMessageHandler<DeliveryApiHeadersHandler>()
                .AddHeaderPropagation();

            // Preview
            builder.Services.AddRefitGeneratedClient<IPreviewVerificationApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/api/v1.0/preview");
                })
                .AddHeaderPropagation();

            // Sitemap
            builder.Services.AddRefitGeneratedClient<ISitemapsApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/api/v1.0/pages");
                })
                .AddHttpMessageHandler<DeliveryApiHeadersHandler>()
                .AddHeaderPropagation();

            // Robots
            builder.Services.AddRefitGeneratedClient<IRobotsApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new($"https://{Services.Cms}/api/v1.0/pages");
                })
                .AddHttpMessageHandler<DeliveryApiHeadersHandler>()
                .AddHeaderPropagation();
        }
    }

    extension(IList<JsonConverter> convertersList)
    {
        public void AddDeliveryApiConverters()
        {
            convertersList.Add(new ApiElementConverter());
            convertersList.Add(new ApiContentConverter());
        }
    }
}
