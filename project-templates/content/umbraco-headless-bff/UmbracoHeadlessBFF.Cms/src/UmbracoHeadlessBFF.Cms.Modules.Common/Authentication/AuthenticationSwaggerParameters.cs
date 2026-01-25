using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Core.Configuration.Models;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Authentication;

public sealed class AuthenticationSwaggerParameters : IOperationFilter
{
    private readonly IOptionsMonitor<DeliveryApiSettings> _deliveryApiSettings;

    public AuthenticationSwaggerParameters(IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _deliveryApiSettings = deliveryApiSettings;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        var apiKeyParameter = operation.Parameters.FirstOrDefault(x => x.Name == DeliveryApiConstants.ApiKeyHeaderName);

        if (apiKeyParameter is null || _deliveryApiSettings.CurrentValue is { PublicAccess: true })
        {
            return;
        }

        var param = apiKeyParameter as OpenApiParameter;
        param?.Required = true;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = DeliveryApiConstants.ApiKeyHeaderName,
            Description = "The api key to be used when making requests",
            In = ParameterLocation.Header,
            Required = true,
        });

        return;
    }
}
