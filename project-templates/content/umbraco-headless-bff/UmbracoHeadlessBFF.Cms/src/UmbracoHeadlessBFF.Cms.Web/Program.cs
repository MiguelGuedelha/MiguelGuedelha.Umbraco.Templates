using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;
using Umbraco.Community.DataProtection.Composing;
using UmbracoHeadlessBFF.Cms.Modules.Common.Authentication;
using UmbracoHeadlessBFF.Cms.Modules.Common.Links;
using UmbracoHeadlessBFF.Cms.Modules.Common.LoadBalancing;
using UmbracoHeadlessBFF.Cms.Modules.Common.Preview;
using UmbracoHeadlessBFF.Cms.Modules.Common.Umbraco;
using UmbracoHeadlessBFF.Cms.Modules.Common.Urls;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Correlation;
using UmbracoHeadlessBFF.SharedModules.Common.Environment;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;

var builder = WebApplication.CreateBuilder(args);

var umbracoBuilder = builder.CreateUmbracoBuilder();

umbracoBuilder
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddAzureBlobMediaFileSystem()
    .AddAzureBlobImageSharpCache();

umbracoBuilder.AddUmbracoDataProtection();

umbracoBuilder.AddLoadBalancing();

umbracoBuilder.WebhookEvents().Clear().AddCms(false, WebhookPayloadType.Extended);

if (!builder.Environment.IsLocal())
{
    // Docker related disable
    umbracoBuilder.RuntimeModeValidators().Remove<UseHttpsValidator>();
}

umbracoBuilder.Build();

builder.AddServiceDefaults();

builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<AuthenticationSwaggerParameters>();
});

// Adds SiteAPI caches for refresh/clear purposes only
builder.AddCachingSharedModule(CachingConstants.SiteApi.CacheName, true);

builder.AddCorrelationCommonSharedModule();
builder.AddPreviewModule();
builder.AddLinksModule();
builder.AddUrlsModule();

builder.Services.Configure<KestrelServerOptions>(o =>
{
    o.AddServerHeader = false;
});

builder.Services.AddControllers().AddJsonOptions(Constants.JsonOptionsNames.DeliveryApi, options =>
{
    options.JsonSerializerOptions.MaxDepth = 128;
});

if (builder.Environment.IsLocal())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddAzureClients(clientBuilder =>
{
    var blobConnectionString = builder.Configuration["Umbraco:Storage:AzureBlob:Media:ConnectionString"];

    if (string.IsNullOrWhiteSpace(blobConnectionString))
    {
        throw new ArgumentException("No blob container connection string");
    }

    clientBuilder.AddBlobServiceClient(blobConnectionString);
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    await next();
});

await app.BootUmbracoAsync();

app.UseCorrelationSharedModules();

app.MapDefaultEndpoints();

if (!app.Environment.IsProduction())
{
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/umbraco/swagger/{documentName}/swagger.json";
    });
}

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

var version = AssemblyVersionExtensions.GetVersion();
app.MapGet("/version", () => new { version })
    .WithTags("Version");

await app.RunAsync();
