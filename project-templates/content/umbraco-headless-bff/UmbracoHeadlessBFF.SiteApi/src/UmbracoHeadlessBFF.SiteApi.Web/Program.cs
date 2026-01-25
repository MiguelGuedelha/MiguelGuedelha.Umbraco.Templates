using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Scalar.AspNetCore;
using Serilog;
using UmbracoHeadlessBFF.SharedModules.Cms;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Correlation;
using UmbracoHeadlessBFF.SharedModules.Common.Environment;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Errors;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages;
using UmbracoHeadlessBFF.SiteApi.Web.Swagger;
using CachingConstants = UmbracoHeadlessBFF.SharedModules.Common.Caching.CachingConstants;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

if (environment.IsLocal())
{
    configuration.AddUserSecrets<Program>();
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateBootstrapLogger();

try
{
    builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // add a custom operation filter which sets default values
        options.OperationFilter<SwaggerDefaultValues>();
        options.OperationFilter<CorrelationSwaggerParameters>();
    });

    builder.Services.AddSerilog((sc, lc) => lc
        .ReadFrom.Configuration(configuration)
        .ReadFrom.Services(sc));

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.Converters.AddDeliveryApiConverters();
        options.SerializerOptions.Converters.AddPagesConverters();
    });

    builder.AddServiceDefaults();

    builder.AddCachingSharedModule(CachingConstants.SiteApi.CacheName,
        namedCache: true,
        configureJsonSerializerOptions: options =>
        {
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.AddDeliveryApiConverters();
            options.Converters.AddPagesConverters();
        });

    builder.AddCorrelationCommonSharedModule();
    builder.AddCmsCommonSharedModule();
    builder.AddErrorsCommonModule();
    builder.AddCachingCommonModule();
    builder.AddCmsCommonModule();
    builder.AddPagesModule();

    builder.Services.Configure<KestrelServerOptions>(o =>
    {
        o.AddServerHeader = false;
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseStatusCodePages();

    app.UseCorrelationSharedModules();

    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.MapScalarApiReference(options =>
        {
            var descriptions = app.DescribeApiVersions();

            var documents = descriptions
                .Select(x => new ScalarDocument(
                    x.GroupName.ToUpperInvariant(),
                    $"Content API - {x.GroupName}",
                    $"/swagger/{x.GroupName}/swagger.json")
                );

            options.AddDocuments(documents);

            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseExceptionHandler();

    app.UseCmsCommonModuleMiddleware();
    app.UseOutputCache();

    app.MapDefaultEndpoints();

    var apiVersionSet = app
        .MapGroup("/api")
        .NewApiVersionSet()
        .HasApiVersion(1.0)
        .ReportApiVersions()
        .Build();

    var versionGroup = app
        .MapGroup("/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    versionGroup.MapPagesEndpoints();

    if (app.Environment.IsLocal())
    {
        app
            .MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
                string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)))
            .WithTags("Debug");
    }

    var version = AssemblyVersionExtensions.GetVersion();
    app.MapGet("/version", () => new { version }).WithTags("Version");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
