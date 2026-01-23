using Microsoft.Extensions.Configuration;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Environment;
using UmbracoHeadlessBFF.SharedModules.Common.ServiceDiscovery;

var builder = DistributedApplication.CreateBuilder(args);

if (builder.Environment.IsLocal())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var smtpUser = builder.AddParameter("SmtpUser");
var smtpPassword = builder.AddParameter("SmtpPassword", true);
var smtpPort = builder.AddParameter("SmtpPort");

var smtpPortString = await smtpPort.Resource.GetValueAsync(CancellationToken.None);

const string baseBindPath = "../../../local-data/v17/";

var mailServer = builder
    .AddMailPit(Services.SmtpServer, 35000, int.Parse(smtpPortString!))
    .WithArgs("--smtp-auth-allow-insecure")
    .WithDataBindMount(Path.Join(baseBindPath, "mailpit/data"))
    .WithUrlForEndpoint("http", x =>
    {
        x.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        x.DisplayText = "Mail Server UI";
    })
    .WithUrlForEndpoint("smtp", x =>
    {
        x.DisplayLocation = UrlDisplayLocation.DetailsOnly;
    })
    .WithEnvironment("MP_SMTP_AUTH", $"{smtpUser}:{smtpPassword}");

smtpUser.WithParentRelationship(mailServer);
smtpPassword.WithParentRelationship(mailServer);
smtpPort.WithParentRelationship(mailServer);

var database = builder
    .AddSqlServer(Services.DatabaseServer)
    .WithDataBindMount(Path.Join(baseBindPath, "database/data"))
    .WithContainerRuntimeArgs("--user", "root")
    .WithUrlForEndpoint("tcp", x => { x.DisplayLocation = UrlDisplayLocation.DetailsOnly; });

var umbracoDb = database.AddDatabase(Services.Database, "umbraco-cms");

var cache = builder
    .AddRedis(CachingConstants.ConnectionStringName)
    .WithUrlForEndpoint("tcp", x => { x.DisplayLocation = UrlDisplayLocation.DetailsOnly; })
    .WithRedisInsight(c =>
    {
        c.WithDataBindMount(Path.Join(baseBindPath, "redis-insight/data"));
        c.WithUrlForEndpoint("http", x =>
        {
            x.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
            x.DisplayText = "Redis Insight UI";
        });
    });

var azureStorage = builder
    .AddAzureStorage(Services.AzureStorage)
    .RunAsEmulator(o =>
    {
        o.WithDataBindMount(Path.Join(baseBindPath, "azure-storage/data"));
    })
    .WithUrlForEndpoint("blob", x => { x.DisplayLocation = UrlDisplayLocation.DetailsOnly; })
    .WithUrlForEndpoint("queue", x => { x.DisplayLocation = UrlDisplayLocation.DetailsOnly; })
    .WithUrlForEndpoint("table", x => { x.DisplayLocation = UrlDisplayLocation.DetailsOnly; });


var cmsUmbracoBlobContainerNameParameter = builder.AddParameter("CmsUmbracoBlobContainer");
var blobContainerValue = await cmsUmbracoBlobContainerNameParameter.Resource.GetValueAsync(CancellationToken.None);

var umbracoMediaBlob = azureStorage.AddBlobContainer(blobContainerValue!);

cmsUmbracoBlobContainerNameParameter.WithParentRelationship(umbracoMediaBlob);

var cms = builder.AddProject<Projects.Cms>(Services.Cms)
    .WithExternalHttpEndpoints();

var cmsDeliveryApiKey = builder.AddParameter("CmsDeliveryApiKey");

var azureBlobStorageResource = umbracoMediaBlob.Resource.Parent;

cms.WithReference(umbracoDb, connectionName: "umbracoDbDSN")
    .WithReference(cache)
    .WithEnvironment("Umbraco__CMS__Global__Smtp__Host", "localhost")
    .WithEnvironment("Umbraco__CMS__Global__Smtp__Port", smtpPort)
    .WithEnvironment("Umbraco__CMS__Global__Smtp__Username", smtpUser)
    .WithEnvironment("Umbraco__CMS__Global__Smtp__Password", smtpPassword)
    .WithEnvironment("Umbraco__Storage__AzureBlob__Media__ConnectionString", azureBlobStorageResource)
    .WithEnvironment("Umbraco__Storage__AzureBlob__Media__ContainerName", cmsUmbracoBlobContainerNameParameter)
    .WithEnvironment("Umbraco__Storage__AzureBlob__TemporaryFile__ConnectionString", azureBlobStorageResource)
    .WithEnvironment("Umbraco__Storage__AzureBlob__TemporaryFile__ContainerName", cmsUmbracoBlobContainerNameParameter)
    .WithEnvironment("Umbraco__CMS__DeliveryApi__ApiKey", cmsDeliveryApiKey)
    .WithEnvironment("Umbraco__Storage__Cdn__Url", () => cms.Resource.GetEndpoint("https").Url)
    .WaitFor(mailServer)
    .WaitFor(umbracoDb)
    .WaitFor(cache)
    .WaitFor(umbracoMediaBlob);

cmsDeliveryApiKey.WithParentRelationship(cms);

// Scalar URLs exists at endpoint/scalar/<group-name>
// i.e /scalar/default, /scalar/management, /scalar/delivery, etc
cms.WithUrlForEndpoint("https", x =>
{
    x.DisplayLocation = UrlDisplayLocation.DetailsOnly;
    x.DisplayOrder = 9999;
});

cms.WithUrls(x =>
{
    var httpsUrl = x.GetEndpoint("https")?.Url;

    x.Urls.Add(new()
    {
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
        DisplayText = "Umbraco Dashboard",
        Url = $"{httpsUrl}/umbraco",
        DisplayOrder = 50
    });

    x.Urls.Add(new()
    {
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
        DisplayText = "Swagger - Delivery API",
        Url = $"{httpsUrl}/umbraco/swagger/index.html?urls.primaryName=Umbraco+Delivery+API",
        DisplayOrder = 10
    });

    x.Urls.Add(new()
    {
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
        DisplayText = "Scalar - Default API",
        Url = $"{httpsUrl}/scalar/default",
        DisplayOrder = 9
    });
});

var siteApi = builder.AddProject<Projects.SiteApi>(Services.SiteApi)
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(cms)
    .WithEnvironment("services__Cms__Parameters__DeliveryApiKey", cmsDeliveryApiKey)
    .WaitFor(cache)
    .WaitFor(cms);

siteApi.WithUrlForEndpoint("https", x =>
{
    x.DisplayLocation = UrlDisplayLocation.DetailsOnly;
    x.DisplayOrder = 9999;
});

siteApi.WithUrls(x =>
{
    var httpsUrl = x.GetEndpoint("https")?.Url;

    x.Urls.Add(new()
    {
        DisplayText = "Scalar",
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
        Url = $"{httpsUrl}/scalar",
        DisplayOrder = 50
    });
});

await builder.Build().RunAsync();
